using System;

namespace ContaFlow.API.Features.Recibos
{
    public static class NumeroALetras
    {
        public static string Convertir(decimal numero)
        {
            long entero = (long)Math.Truncate(numero);
            int centavos = (int)Math.Round((numero - entero) * 100);

            string letras = ToLetras(entero);
            return $"{letras.ToUpper()} LEMPIRAS CON {centavos:00}/100 CTVS.";
        }

        private static string ToLetras(long value)
        {
            if (value == 0) return "CERO";
            if (value < 0) return "MENOS " + ToLetras(Math.Abs(value));

            string num2Text = "";

            if ((value / 1000000) > 0)
            {
                if (value / 1000000 == 1)
                    num2Text += "UN MILLON ";
                else
                    num2Text += ToLetras(value / 1000000) + " MILLONES ";
                value %= 1000000;
            }

            if ((value / 1000) > 0)
            {
                if (value / 1000 == 1)
                    num2Text += "MIL ";
                else
                    num2Text += ToLetras(value / 1000) + " MIL ";
                value %= 1000;
            }

            if ((value / 100) > 0)
            {
                if (value == 100)
                    num2Text += "CIEN ";
                else if (value / 100 == 1)
                    num2Text += "CIENTO ";
                else if (value / 100 == 5)
                    num2Text += "QUINIENTOS ";
                else if (value / 100 == 7)
                    num2Text += "SETECIENTOS ";
                else if (value / 100 == 9)
                    num2Text += "NOVECIENTOS ";
                else
                    num2Text += ToLetras(value / 100) + "CIENTOS ";
                value %= 100;
            }

            if (value > 0)
            {
                string[] unidades = { "CERO", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE", "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE", "VEINTE" };
                string[] decenas = { "", "", "VEINTI", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };

                if (value <= 20)
                    num2Text += unidades[value];
                else
                {
                    num2Text += decenas[value / 10];
                    if ((value % 10) > 0)
                    {
                        if (value / 10 == 2)
                            num2Text += unidades[value % 10].ToLower();
                        else
                            num2Text += " Y " + unidades[value % 10];
                    }
                }
            }

            return num2Text.Trim();
        }
    }
}
