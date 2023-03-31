using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace stegan
{
    internal class main
    {
        static Matrix H = new Matrix(new int[,] { { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 },
                                                  { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1 },
                                                  { 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 },
                                                  { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 } });
        static void Main(string[] args)
        {
            //Локальные адреса картинок
            string source = @"C:\Users\ceme4\source\repos\stegan\source.bmp";
            string target = @"C:\Users\ceme4\source\repos\stegan\target.bmp";
            string result = @"C:\Users\ceme4\source\repos\stegan\result.bmp";
            string result_u = @"C:\Users\ceme4\source\repos\stegan\result_u.bmp";

            var bmp = Embedding(source, target, result);
            Extract(bmp, result_u);
        }

        /// <summary>
        /// Кодирует картинку target в картинку source.
        /// </summary>
        /// <param name="source">Адресс начальное картинки</param>
        /// <param name="target">Адресс кодируемой картинки</param>
        /// <param name="result">Адресс полученное картинки</param>
        /// <returns>Возвращает полученную картинку.</returns>        
        public static Bitmap Embedding(string source, string target, string result)
        {
            int t_i = 0;
            int t_j = 0;

            Bitmap sourceImg = new Bitmap(source); 
            Bitmap targetImg = new Bitmap(target);

            if (targetImg.Height * 2 > sourceImg.Height || targetImg.Width * 2 > sourceImg.Width)
                throw new Exception("Picture is too small");

            for (int i = 0; i < sourceImg.Height; i += 2)
            {
                for (int j = 0; j < sourceImg.Width; j += 2)
                {
                    bool flag = (t_i < targetImg.Height && t_j < targetImg.Width);

                    //Беру пиксели квадратом 2х2

                    var pix1 = sourceImg.GetPixel(i, j);
                    var pix2 = sourceImg.GetPixel(i, j + 1);
                    var pix3 = sourceImg.GetPixel(i + 1, j);
                    var pix4 = sourceImg.GetPixel(i + 1, j + 1);

                    var pix = flag ? targetImg.GetPixel(t_i, t_j) : Color.Black; // Проверяю, находится ли координата пикселя в рамках кодируемой картинки, иначе беру чёрный пиксель

                    var pixelArr = GetMergedPixels(pix1, pix2, pix3, pix4, pix); // Получаю пиксели, в которых закодирован пиксель кодируемой картинки

                    sourceImg.SetPixel(i, j, pixelArr[0]);
                    sourceImg.SetPixel(i, j + 1, pixelArr[1]);
                    sourceImg.SetPixel(i + 1, j, pixelArr[2]);
                    sourceImg.SetPixel(i + 1, j + 1, pixelArr[3]);

                    t_j += 1;
                }
               
                t_j = 0;
                t_i += 1;
            }
            
            sourceImg.Save(result);

            return sourceImg;
        }

        public static Bitmap Extract(Bitmap bmp, string result)
        {
            int t_i = 0;
            int t_j = 0;

            Bitmap sourceImg = bmp;
            Bitmap resultImg = new Bitmap(sourceImg.Width/2,sourceImg.Height/2);

            for (int i = 0; i < sourceImg.Height; i += 2)
            {
                for (int j = 0; j < sourceImg.Width; j += 2)
                {
                    var pix1 = sourceImg.GetPixel(i, j);
                    var pix2 = sourceImg.GetPixel(i, j + 1);
                    var pix3 = sourceImg.GetPixel(i + 1, j);
                    var pix4 = sourceImg.GetPixel(i + 1, j + 1);

                    resultImg.SetPixel(t_i, t_j, GetUnmergedPixel(pix1, pix2, pix3, pix4)); // Получаю из закодированных пикселей исходной картинки пиксель кодируемой картинки

                    t_j += 1;
                }
                
                t_j = 0;
                t_i += 1;
            }

            resultImg.Save(result);

            return resultImg;
        }    

        /// <summary>
        /// Передов элементов массива из 10 в 2 систему счисления
        /// </summary>
        /// <param name="array">Массив целых чисел в диапазоне 0-255</param>
        /// <returns>Массив двоичных чисел</returns>
        public static int[] GetBinary(int[] array)
        {
            int[] bin = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                bin[i] = Convert.ToInt32(Convert.ToString(array[i], 2));
            }
            return bin;
        }

        /// <summary>
        /// Разюивает строку бит на массив бит
        /// </summary>
        /// <param name="code">Строка бит</param>
        /// <returns>Массив бит</returns>
        public static int[] GetBits(string code)
        {
            int[] bits = new int[code.Length];
            for(int i = 0;i < code.Length;i++)
            {
                bits[i] = Convert.ToInt32(Char.GetNumericValue(code[i]));
            }
            return bits;
        }

        /// <summary>
        /// Приводит все числа в матрице к бинарному виду
        /// </summary>
        /// <param name="matrix">Матрицы целых чисел</param>
        /// <returns>Матрица бит</returns>
        public static Matrix GetBinaryMatrix(Matrix matrix)
        {
            for (int i = 0; i < matrix.array.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.array.GetLength(1); j++)
                {
                    matrix.array[i, j] = matrix.array[i, j] % 2;
                }
            }
            return matrix;
        }

        /// <summary>
        /// Вспомогательная функция. Превращает матрицу с одной строкой в обычный массив
        /// </summary>
        /// <param name="array">Матрица с одной строкой</param>
        /// <returns>Массив</returns>
        public static int[] GetArray(int[,] array)
        {
            int[] arr = new int[array.GetLength(1)];
            if (array.GetLength(0) == 1)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = array[0, i];
                }
            }
            return arr;
        }

        /// <summary>
        /// Меняет i-тый бит в массиве бит
        /// </summary>
        /// <param name="arr">Массив бит</param>
        /// <param name="pos">Позиция заменяемого бита</param>
        public static void ChangeBit(int[] arr, int pos)
        {     
            if (pos <= 0)
                return;
            else if (arr[pos-1] == 1)
                arr[pos-1] = 0;
            else
                arr[pos-1] = 1;
        }

        /// <summary>
        /// Перевод из двоичного числа в десятичное
        /// </summary>
        /// <param name="arr">Массив двоичных чисел</param>
        /// <returns>Массив целых чисел</returns>
        public static int BinToInt(int[] arr)
        {
            int res = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                res += arr[i] * (int)Math.Pow(2, arr.Length - 1 - i);
            }
            return res;
        }
                
        /// <summary>
        /// Приводит двоичное число к 8-битному виду
        /// </summary>
        /// <param name="code">Строка бит</param>
        /// <returns>Строка из 8 бит</returns>
        public static string GetRightCode(string code)
        {
            var length = code.Length;
            if (code.Length < 8)
                for (int i = 0; i < 8 - length; i++)
                {
                    code = 0 + code;
                }
            return code;
        }

        /// <summary>
        /// Получает исходный пиксель из четырёх закодированных
        /// </summary>
        /// <param name="pix1">Пиксель 1</param>
        /// <param name="pix2">Пиксель 2</param>
        /// <param name="pix3">Пиксель 3</param>
        /// <param name="pix4">Пиксель 4</param>
        /// <returns>Исходный пиксель</returns>
        public static Color GetUnmergedPixel(Color pix1, Color pix2, Color pix3, Color pix4)
        {
            // Массивы цветов пикселей
            var rgb1 = GetBinary(new int[] { pix1.R, pix1.G, pix1.B });
            var rgb2 = GetBinary(new int[] { pix2.R, pix2.G, pix2.B });
            var rgb3 = GetBinary(new int[] { pix3.R, pix3.G, pix3.B });
            var rgb4 = GetBinary(new int[] { pix4.R, pix4.G, pix4.B });

            // Красные цвета пикселей 
            var r1 = rgb1[0];
            var r2 = rgb2[0];
            var r3 = rgb3[0];
            var r4 = rgb4[0];

            // Зелёные цвета пикселей 
            var g1 = rgb1[1];
            var g2 = rgb2[1];
            var g3 = rgb3[1];
            var g4 = rgb4[1];

            // Синие цвета пикселей 
            var b1 = rgb1[2];
            var b2 = rgb2[2];
            var b3 = rgb3[2];
            var b4 = rgb4[2];            

            // Строю контейнеры по 15 символов, беря 3 последних бита цвета первого пикселя и по 4 последних бита у остальных пикселей
            var r_cont = GetBits((GetRightCode(r1.ToString()))[^3..] + (GetRightCode(r2.ToString()))[^4..] + (GetRightCode(r3.ToString()))[^4..] + (GetRightCode(r4.ToString())[^4..]));
            var g_cont = GetBits((GetRightCode(g1.ToString()))[^3..] + (GetRightCode(g2.ToString()))[^4..] + (GetRightCode(g3.ToString()))[^4..] + (GetRightCode(g4.ToString())[^4..]));
            var b_cont = GetBits((GetRightCode(b1.ToString()))[^3..] + (GetRightCode(b2.ToString()))[^4..] + (GetRightCode(b3.ToString()))[^4..] + (GetRightCode(b4.ToString())[^4..]));

            // Получаю цвета исхоного пикселя в двочном виде
            var r_cont_matr = new Matrix(r_cont); // превращаю контейнер в матрицу
            var r_mult = H * r_cont_matr.Transpose(); // умножаю матрицу H на контейнер
            var r_matr = GetBinaryMatrix(r_mult).Transpose(); // <- исходный красный

            var g_cont_matr = new Matrix(g_cont);
            var g_mult = H * g_cont_matr.Transpose();
            var g_matr = GetBinaryMatrix(g_mult).Transpose(); // <- исходный зелёный

            var b_cont_matr = new Matrix(b_cont);
            var b_mult = H * b_cont_matr.Transpose();
            var b_matr = GetBinaryMatrix(b_mult).Transpose(); // <- исходный синий

            // Получаю цвета исходного пикселя в десятичном виде
            var r = BinToInt(GetBits(String.Concat(GetArray(r_matr.array)) + "0000"));
            var g = BinToInt(GetBits(String.Concat(GetArray(g_matr.array)) + "0000"));
            var b = BinToInt(GetBits(String.Concat(GetArray(b_matr.array)) + "0000"));

            // Возвращаю исходный пиксель
            return Color.FromArgb(r,g,b);
        }

        /// <summary>
        /// Кодирует пиксель в 4 
        /// </summary>
        /// <param name="pix1">Пиксель 1</param>
        /// <param name="pix2">Пиксель 2</param>
        /// <param name="pix3">Пиксель 3</param>
        /// <param name="pix4">Пиксель 4</param>
        /// <param name="pix">Кодируемый пиксель</param>
        /// <returns>4 закодированных пикселей</returns>
        public static Color[] GetMergedPixels(Color pix1, Color pix2, Color pix3, Color pix4, Color pix)
        {
            // Массивы цветов пикселей
            var rgb1 = GetBinary(new int[] { pix1.R, pix1.G, pix1.B });
            var rgb2 = GetBinary(new int[] { pix2.R, pix2.G, pix2.B });
            var rgb3 = GetBinary(new int[] { pix3.R, pix3.G, pix3.B });
            var rgb4 = GetBinary(new int[] { pix4.R, pix4.G, pix4.B });
            var rgb = GetBinary(new int[] { pix.R, pix.G, pix.B });

            // Красные цвета пикселей 
            var r1 = rgb1[0];
            var r2 = rgb2[0];
            var r3 = rgb3[0];
            var r4 = rgb4[0];
            var r = rgb[0];

            // Зелёные цвета пикселей 
            var g1 = rgb1[1];
            var g2 = rgb2[1];
            var g3 = rgb3[1];
            var g4 = rgb4[1];
            var g = rgb[1];

            // Синие цвета пикселей 
            var b1 = rgb1[2];
            var b2 = rgb2[2];
            var b3 = rgb3[2];
            var b4 = rgb4[2];
            var b = rgb[2];

            // Строю контейнеры по 15 символов, беря 3 последних бита цвета первого пикселя и по 4 последних бита у остальных пикселей
            var r_cont = GetBits((GetRightCode(r1.ToString()))[^3..] + (GetRightCode(r2.ToString()))[^4..] + (GetRightCode(r3.ToString()))[^4..] + (GetRightCode(r4.ToString())[^4..]));
            var g_cont = GetBits((GetRightCode(g1.ToString()))[^3..] + (GetRightCode(g2.ToString()))[^4..] + (GetRightCode(g3.ToString()))[^4..] + (GetRightCode(g4.ToString())[^4..]));
            var b_cont = GetBits((GetRightCode(b1.ToString()))[^3..] + (GetRightCode(b2.ToString()))[^4..] + (GetRightCode(b3.ToString()))[^4..] + (GetRightCode(b4.ToString())[^4..]));
                          
            // Получаю первые 4 бита цветов кодируемого пикселя
            var r_code = GetBits((GetRightCode(r.ToString()))[..4]);
            var g_code = GetBits((GetRightCode(g.ToString()))[..4]);
            var b_code = GetBits((GetRightCode(b.ToString()))[..4]);

            // Получаю матрицы H(x~)^T
            var r_cont_matr = new Matrix(r_cont);
            var r_mult = H * r_cont_matr.Transpose(); 
            var r_matr = GetBinaryMatrix(r_mult).Transpose();

            var g_cont_matr = new Matrix(g_cont);
            var g_mult = H * g_cont_matr.Transpose();
            var g_matr = GetBinaryMatrix(g_mult).Transpose();

            var b_cont_matr = new Matrix(b_cont);
            var b_mult = H * b_cont_matr.Transpose();
            var b_matr = GetBinaryMatrix(b_mult).Transpose();

            // Получаю синдромы
            var sub_r = GetBinaryMatrix(new Matrix(r_code) - r_matr);
            var sub_g = GetBinaryMatrix(new Matrix(g_code) - g_matr);
            var sub_b = GetBinaryMatrix(new Matrix(b_code) - b_matr);

            // Получаю ошибки в соответствующих координатах
            ChangeBit(r_cont, BinToInt(GetArray(sub_r.array)));
            ChangeBit(g_cont, BinToInt(GetArray(sub_g.array)));
            ChangeBit(b_cont, BinToInt(GetArray(sub_b.array)));

            // Перевожу контейнеры в строку для более удобного использования
            var r_cont_str = String.Concat(r_cont);
            var g_cont_str = String.Concat(g_cont);
            var b_cont_str = String.Concat(b_cont);

            // Кодирую биты кодируемого пикселя в указанные 4 пикселя
            var new_r1 = GetBits((GetRightCode(r1.ToString()))[..5] + r_cont_str[..3]);
            var new_r2 = GetBits((GetRightCode(r2.ToString()))[..4] + r_cont_str[3..7]);
            var new_r3 = GetBits((GetRightCode(r3.ToString()))[..4] + r_cont_str[7..11]);
            var new_r4 = GetBits((GetRightCode(r4.ToString()))[..4] + r_cont_str[11..]);

            var new_g1 = GetBits((GetRightCode(g1.ToString()))[..5] + g_cont_str[..3]);
            var new_g2 = GetBits((GetRightCode(g2.ToString()))[..4] + g_cont_str[3..7]);
            var new_g3 = GetBits((GetRightCode(g3.ToString()))[..4] + g_cont_str[7..11]);
            var new_g4 = GetBits((GetRightCode(g4.ToString()))[..4] + g_cont_str[11..]);

            var new_b1 = GetBits((GetRightCode(b1.ToString()))[..5] + b_cont_str[..3]);
            var new_b2 = GetBits((GetRightCode(b2.ToString()))[..4] + b_cont_str[3..7]);
            var new_b3 = GetBits((GetRightCode(b3.ToString()))[..4] + b_cont_str[7..11]);
            var new_b4 = GetBits((GetRightCode(b4.ToString()))[..4] + b_cont_str[11..]);
                                
            // Возвращаю массив закодированных пикселей
            return new Color[] {Color.FromArgb(BinToInt(new_r1), BinToInt(new_g1), BinToInt(new_b1)),
                                Color.FromArgb(BinToInt(new_r2), BinToInt(new_g2), BinToInt(new_b2)),
                                Color.FromArgb(BinToInt(new_r3), BinToInt(new_g3), BinToInt(new_b3)),
                                Color.FromArgb(BinToInt(new_r4), BinToInt(new_g4), BinToInt(new_b4))};

        }
    }
}
