using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

class Program
{
    static void Main(string[] args)
    {
        // Diretório onde estão as imagens
        string inputDir = "./imagens/";
        // Diretório onde será salva a imagem combinada
        string outputDir = "./output/";

        // Largura desejada para as imagens
        int desiredWidth = 900;

        // Lista apenas os arquivos JPEG no diretório e ordena-os numericamente
        string[] imageFiles = Directory.GetFiles(inputDir, "*.jpg");
        Array.Sort(imageFiles, CompareFileNamesNumerically);

        // Agrupa as imagens em lotes de 5
        List<string[]> groupedImages = new List<string[]>();
        int groupSize = 5;
        int numGroups = (int)Math.Ceiling((double)imageFiles.Length / groupSize);
        for (int i = 0; i < numGroups; i++)
        {
            int groupStartIndex = i * groupSize;
            int groupEndIndex = Math.Min(groupStartIndex + groupSize, imageFiles.Length);
            string[] group = new string[groupEndIndex - groupStartIndex];
            Array.Copy(imageFiles, groupStartIndex, group, 0, group.Length);
            groupedImages.Add(group);
        }

        // Combina as imagens verticalmente
        int imageIndex = 1;
        foreach (var group in groupedImages)
        {
            int totalHeight = 0;
            int maxWidth = 0;

            // Carrega as imagens e calcula a altura total e a largura máxima
            foreach (string imagePath in group)
            {
                using (var image = Image.Load(imagePath))
                {
                    maxWidth = Math.Max(maxWidth, image.Width);
                    totalHeight += image.Height;
                }
            }

            // Redimensiona as imagens e combina verticalmente
            using (var outputImage = new Image<Rgba32>(maxWidth, totalHeight))
            {
                int y = 0;
                foreach (string imagePath in group)
                {
                    using (var image = Image.Load(imagePath))
                    {
                        // Redimensiona a imagem para a largura desejada
                        image.Mutate(x => x.Resize(desiredWidth, (int)((double)desiredWidth / image.Width * image.Height)));

                        // Combina a imagem na imagem de saída
                        outputImage.Mutate(x => x.DrawImage(image, new Point(0, y), 1f));

                        // Atualiza a posição y para a próxima imagem
                        y += image.Height;
                    }
                }

                // Salva a imagem combinada
                string outputPath = Path.Combine(outputDir, $"combined_{imageIndex}.jpg");
                outputImage.Save(outputPath);
                Console.WriteLine($"Imagem combinada {imageIndex} salva em: {outputPath}");
            }

            imageIndex++;
        }
    }

    // Função de comparação para ordenar os nomes de arquivo numericamente
    static int CompareFileNamesNumerically(string a, string b)
    {
        // Extrai os números dos nomes de arquivo
        int numberA = ExtractNumber(Path.GetFileNameWithoutExtension(a));
        int numberB = ExtractNumber(Path.GetFileNameWithoutExtension(b));
        // Compara os números extraídos
        return numberA.CompareTo(numberB);
    }

    // Função auxiliar para extrair números de strings
    static int ExtractNumber(string s)
    {
        // Extrai os dígitos da string
        string number = "";
        foreach (char c in s)
        {
            if (char.IsDigit(c))
                number += c;
        }
        // Converte a string de dígitos em um número inteiro
        return int.Parse(number);
    }
}
