using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

public static class StringExtension
{
    /// <summary>
    /// Elimina espacios dobles o más entre palabras a un simple espacio.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://stackoverflow.com/questions/4910108/how-to-extend-c-sharp-built-in-types-like-string
    /// </remarks>
    public static string ToSingleSpaces(this string value)
    {
        return Regex.Replace(value, @"\s+", " ");
    } // ToSingleSpaces

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// http://www.forosdelweb.com/f78/como-mayusculas-las-primeras-letras-cada-palabra-c-403861/
    /// </remarks>
    public static string ToCappitalize(this string value)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
    } // ToCappitalize

    //TODO: Ver esto: System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(AreaNombre.ToLower()); // https://stackoverflow.com/questions/748411/is-there-a-capitalizefirstletter-method
    
    /// <summary>
    /// Elimina los caracteres no validos para el nombre de un archivo o carpeta
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Cadena con un nombre de archivo valido</returns>
    /// <remarks>
    /// https://social.msdn.microsoft.com/Forums/es-ES/09fd1b91-93c4-49c0-bcd0-b9fc02582c35/pathgetinvalidfilenamechars-no-detecta-todos-los-caracteres-invlidos?forum=vbes
    /// </remarks>
    public static string CleanInvalidFileNameChars(this string value)
    {
        char[] invalidPathChars = System.IO.Path.GetInvalidFileNameChars();
        StringBuilder chars = new StringBuilder(invalidPathChars.ToString());

        chars.Append("\\");

        try
        {
            string pattern = string.Format("[{0}]", chars.ToString());
            return Regex.Replace(value, pattern, string.Empty).Trim();
        }
        catch
        {
            return string.Empty;
        }
    } // CleanInvalidChars

    /// <summary>
    /// Elimina caracteres que afectan la comparacion de dos cadenas similares
    /// </summary>
    /// <param name="s"></param>
    /// <returns>La cadena sin acentos y en mayusculas</returns>
    /// <remarks>
    /// https://lodesharepoint.com/comparar-strings-sin-importar-mayusculas-minusculas-espacios-ni-acentos-en-c
    /// </remarks>
    public static string PrepareToCompareString(this string s)
    {
        Regex replace_a_Accents = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
        Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
        Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
        Regex replace_o_Accents = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
        Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
        s = replace_a_Accents.Replace(s, "a");
        s = replace_e_Accents.Replace(s, "e");
        s = replace_i_Accents.Replace(s, "i");
        s = replace_i_Accents.Replace(s, "i");
        s = replace_o_Accents.Replace(s, "o");
        s = replace_u_Accents.Replace(s, "u");
        // s = s.ToUpper().Replace(" ", "");
        return s;
    } // prepareToCompareString

} // StringExtension
