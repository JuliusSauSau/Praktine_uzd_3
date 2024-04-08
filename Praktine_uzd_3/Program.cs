using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Pasirinkite veiksmą:");
            Console.WriteLine("1. Užšifruoti ir išsaugoti");
            Console.WriteLine("2. Dešifruoti iš failo");
            Console.WriteLine("3. Baigti programą");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    EncryptAndSave();
                    break;
                case "2":
                    DecryptFromFile();
                    break;
                case "3":
                    Console.WriteLine("Programa baigta.");
                    return;
                default:
                    Console.WriteLine("Pasirinkimas neteisingas.");
                    break;
            }
        }
    }

    static void EncryptAndSave()
    {
        // Pradinių duomenų įvedimas
        BigInteger p = GetPrime();
        BigInteger q = GetPrime();
        BigInteger n = p * q;
        BigInteger phi = (p - 1) * (q - 1);
        BigInteger e = GetPublicKey(phi);
        BigInteger d = GetPrivateKey(e, phi);

        // Viešojo rakto išsaugojimas
        SavePublicKey(n, e);

        // Pradinio teksto įvedimas
        Console.WriteLine("Įveskite tekstą:");
        string plaintext = Console.ReadLine();

        // Pradinio teksto užšifravimas
        BigInteger[] encrypted = Encrypt(plaintext, n, e);

        // Užšifruoto teksto ir viešojo rakto išsaugojimas
        SaveEncryptedText(encrypted);

        Console.WriteLine("Tekstas sėkmingai užšifruotas ir išsaugotas.");
        Console.WriteLine("Grįžti į veiksmų pasirinkimą? (T/N)");
        string response = Console.ReadLine();
        if (response.ToLower() == "n")
            Environment.Exit(0); // Užbaigia programą
    }

    static void DecryptFromFile()
    {
        // Užšifruoto teksto nuskaitymas
        BigInteger[] loadedEncryptedText = LoadEncryptedText();

        // Viešojo rakto nuskaitymas
        BigInteger n, e;
        LoadPublicKey(out n, out e);

        // Privačiojo rakto radimas
        BigInteger p = FindPrimeFactors(n);
        BigInteger q = n / p;
        BigInteger phi = (p - 1) * (q - 1);
        BigInteger d = GetPrivateKey(e, phi);

        // Užšifruoto teksto dešifravimas
        string decryptedText = Decrypt(loadedEncryptedText, d, n);
        Console.WriteLine("Dešifruotas tekstas: " + decryptedText);
    }

    static BigInteger GetPrime()
    {
        Console.Write("Įveskite pirminį skaičių: ");
        while (true)
        {
            string input = Console.ReadLine();
            if (BigInteger.TryParse(input, out BigInteger prime) && IsPrime(prime))
            {
                return prime;
            }
            else
            {
                Console.WriteLine("Įvestas skaičius nėra pirminis. Bandykite dar kartą.");
            }
        }
    }

    static bool IsPrime(BigInteger number)
    {
        if (number <= 1)
            return false;
        if (number <= 3)
            return true;
        if (number % 2 == 0 || number % 3 == 0)
            return false;
        for (BigInteger i = 5; i * i <= number; i += 6)
        {
            if (number % i == 0 || number % (i + 2) == 0)
                return false;
        }
        return true;
    }

    static BigInteger GetPublicKey(BigInteger phi)
    {
        BigInteger e = 65537; // Dažniausiai naudojamas viešasis eksponentės reikšmė
        while (BigInteger.GreatestCommonDivisor(e, phi) != 1)
        {
            e++;
        }
        return e;
    }

    static BigInteger GetPrivateKey(BigInteger e, BigInteger phi)
    {
        BigInteger d = 0;
        BigInteger k = 1;
        while (true)
        {
            d = (k * phi + 1) / e;
            if (e * d % phi == 1)
            {
                break;
            }
            k++;
        }
        return d;
    }

    static BigInteger[] Encrypt(string plaintext, BigInteger n, BigInteger e)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
        BigInteger[] encrypted = new BigInteger[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            BigInteger m = new BigInteger(bytes[i]);
            BigInteger c = BigInteger.ModPow(m, e, n);
            encrypted[i] = c;
        }
        return encrypted;
    }

    static void SavePublicKey(BigInteger n, BigInteger e)
    {
        using (StreamWriter writer = new StreamWriter("public_key.txt"))
        {
            writer.WriteLine(n);
            writer.WriteLine(e);
        }
    }

    static void SaveEncryptedText(BigInteger[] encrypted)
    {
        using (StreamWriter writer = new StreamWriter("encrypted_text.txt"))
        {
            foreach (BigInteger cipher in encrypted)
            {
                writer.WriteLine(cipher);
            }
        }
    }

    static BigInteger[] LoadEncryptedText()
    {
        string[] lines = File.ReadAllLines("encrypted_text.txt");
        BigInteger[] encrypted = new BigInteger[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            encrypted[i] = BigInteger.Parse(lines[i]);
        }
        return encrypted;
    }

    static string Decrypt(BigInteger[] encrypted, BigInteger d, BigInteger n)
    {
        StringBuilder plaintext = new StringBuilder();
        foreach (BigInteger cipher in encrypted)
        {
            BigInteger m = BigInteger.ModPow(cipher, d, n);
            byte[] bytes = m.ToByteArray();
            string decryptedChar = Encoding.UTF8.GetString(bytes);
            plaintext.Append(decryptedChar);
        }
        return plaintext.ToString();
    }

    static void LoadPublicKey(out BigInteger n, out BigInteger e)
    {
        string[] lines = File.ReadAllLines("public_key.txt");
        n = BigInteger.Parse(lines[0]);
        e = BigInteger.Parse(lines[1]);
    }

    static BigInteger FindPrimeFactors(BigInteger n)
    {
        for (BigInteger i = 2; i < n; i++)
        {
            if (n % i == 0 && IsPrime(i))
            {
                return i;
            }
        }
        return 0; // Neįmanoma rasti pirminių faktorių
    }
}
