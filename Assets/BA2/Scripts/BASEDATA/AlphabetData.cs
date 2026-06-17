using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LetterDamageData
{
    public string letter;
    public int baseDamage;
}

[Serializable]
public struct LetterFrequency
{
    public string letter;
    public int weight;
}

public static class AlphabetData
{
    public static List<LetterFrequency> Frequencies;
    public static List<LetterDamageData> DamageTable;
    private static int _totalWeight = 0;

    // El constructor estático inicializa los datos automáticamente una sola vez
    static AlphabetData()
    {
        InitializeFrequencies();
        InitializeDamageTable();
        CalculateTotalWeight();
    }

    private static void InitializeFrequencies()
    {
        // Se unificó la frecuencia de la 'Ñ' (1) con la 'N' (6), dando un peso total de 7 para la ficha 'N'
        Frequencies = new List<LetterFrequency>
        {
            new() { letter = "A", weight = 12 }, new() { letter = "B", weight = 2 },
            new() { letter = "C", weight = 4 },  new() { letter = "D", weight = 5 },
            new() { letter = "E", weight = 12 }, new() { letter = "F", weight = 2 },
            new() { letter = "G", weight = 3 },  new() { letter = "H", weight = 2 },
            new() { letter = "I", weight = 9 },  new() { letter = "J", weight = 1 },
            new() { letter = "K", weight = 1 },  new() { letter = "L", weight = 4 },
            new() { letter = "M", weight = 2 },  new() { letter = "N", weight = 7 }, // N + Ñ
            new() { letter = "O", weight = 8 },  new() { letter = "P", weight = 2 },
            new() { letter = "Qu", weight = 1 }, new() { letter = "R", weight = 6 },
            new() { letter = "S", weight = 4 },  new() { letter = "T", weight = 6 },
            new() { letter = "U", weight = 4 },  new() { letter = "V", weight = 2 },
            new() { letter = "W", weight = 1 },  new() { letter = "X", weight = 1 },
            new() { letter = "Y", weight = 2 },  new() { letter = "Z", weight = 1 }
        };
    }

    private static void InitializeDamageTable()
    {
        DamageTable = new List<LetterDamageData>
        {
            new() { letter = "A", baseDamage = 1 }, new() { letter = "B", baseDamage = 3 },
            new() { letter = "C", baseDamage = 3 }, new() { letter = "D", baseDamage = 2 },
            new() { letter = "E", baseDamage = 1 }, new() { letter = "F", baseDamage = 4 },
            new() { letter = "G", baseDamage = 2 }, new() { letter = "H", baseDamage = 4 },
            new() { letter = "I", baseDamage = 1 }, new() { letter = "J", baseDamage = 8 },
            new() { letter = "K", baseDamage = 5 }, new() { letter = "L", baseDamage = 1 },
            new() { letter = "M", baseDamage = 3 }, new() { letter = "N", baseDamage = 2 }, // Ajustado por la Ñ
            new() { letter = "O", baseDamage = 1 }, new() { letter = "P", baseDamage = 3 },
            new() { letter = "Qu", baseDamage = 6 }, new() { letter = "R", baseDamage = 1 },
            new() { letter = "S", baseDamage = 1 }, new() { letter = "T", baseDamage = 1 },
            new() { letter = "U", baseDamage = 1 }, new() { letter = "V", baseDamage = 4 },
            new() { letter = "W", baseDamage = 8 }, new() { letter = "X", baseDamage = 8 },
            new() { letter = "Y", baseDamage = 4 }, new() { letter = "Z", baseDamage = 10 }
        };
    }

    private static void CalculateTotalWeight()
    {
        _totalWeight = 0;
        foreach (var item in Frequencies) _totalWeight += item.weight;
    }

    public static string GetRandomLetter()
    {
        int randomValue = UnityEngine.Random.Range(0, _totalWeight);
        foreach (var item in Frequencies)
        {
            randomValue -= item.weight;
            if (randomValue < 0) return item.letter;
        }
        return "A";
    }

    public static int GetBaseDamage(string letter)
    {
        // 2. Normalizamos la letra entrante antes de buscarla en la tabla de daños
        string normalizedLetter = NormalizeLetter(letter);

        var data = DamageTable.Find(d => d.letter.Equals(normalizedLetter, StringComparison.OrdinalIgnoreCase));
        return data.letter != null ? data.baseDamage : 1;
    }

    
    // Convierte variantes de caracteres (tildes, diéresis, Ñ) a su letra base representativa. 
    public static string NormalizeLetter(string letter)
    {
        if (string.IsNullOrEmpty(letter)) return "A";

        string upper = letter.ToUpper();

        return upper switch
        {
            "Á" => "A",
            "É" => "E",
            "Í" => "I",
            "Ó" => "O",
            "Ú" or "Ü" => "U", // Triple caso para la U
            "Ñ" => "N",        // Doble caso para la N
            _ => upper
        };
    }
}