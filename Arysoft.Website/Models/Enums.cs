using System.ComponentModel.DataAnnotations;

/// <summary>
/// Permite indicar si el registro para la aplicación se considera eliminado
/// </summary>
public enum StatusTipo
{
    [Display(Name = "(seleccionar status)")]
    Ninguno,
    Activo,
    Baja,
    Eliminado
}

/// <summary>
/// Tipo booleano considerando el primer valor como vacio (tres estados, null, true y false)
/// </summary>
public enum BoolTipo
{
    [Display(Name = "(seleccionar)")]
    Ninguno,
    Si,
    No
}

public enum IdiomaTipo
{ 
    [Display(Name = "(idioma)")]
    Ninguno,
    [Display(Name = "Español")]
    Espannol,
    [Display(Name = "Inglés")]
    Ingles
}