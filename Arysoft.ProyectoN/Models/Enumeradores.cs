using System.ComponentModel.DataAnnotations;

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

/// <summary>
/// Permite indicar la afinidad de una persona con el ProyectoN
/// </summary>
public enum AfinidadTipo
{
    [Display(Name = "(seleccionar afinidad)")]
    Ninguno,
    Movilizador,
    [Display(Name = "Por sector")]
    PorSector,
    [Display(Name = "Por 1x10")]
    PorAfiliado,
    Simpatizante    
} // AfinidadTipo

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
} // RegistroTipo

/// <summary>
/// Establece el tipo de casilla a referenciar
/// </summary>
public enum CasillaTipo
{
    /// <summary>
    /// Considerado valor nulo
    /// </summary>
    [Display(Name = "(seleccionar tipo)")]
    Ninguno,
    /// <summary>
    /// Se instalan en secciones que tienen un número no mayor a 750 electores
    /// </summary>
    [Display(Name = "Básica")]
    Basica,
    /// <summary>
    /// Si la sección rebasa los 750 electores se instalarán casillas contiguas
    /// </summary>
    [Display(Name = "Contigua 1")]
    Contigua,
    /// <summary>
    /// Si la sección rebasa los 1500 electores
    /// </summary>
    [Display(Name = "Contigua 2")]
    ContiguaII,
    /// <summary>
    /// Si la sección rebasa los 2250 electores
    /// </summary>
    [Display(Name = "Contigua 3")]
    ContiguaIII,
    /// <summary>
    /// Si la sección rebasa los 3000 electores
    /// </summary>
    [Display(Name = "Contigua 4")]
    ContiguaIV,
    [Display(Name = "Contigua 5")]
    ContiguaV,
    [Display(Name = "Contigua 6")]
    ContiguaVI,
    [Display(Name = "Contigua 7")]
    ContiguaVII,
    [Display(Name = "Contigua 8")]
    ContiguaVIII,
    [Display(Name = "Contigua 9")]
    ContiguaIX,
    [Display(Name = "Contigua 10")]
    ContiguaX,
    /// <summary>
    /// Para electores que se encuentran fuera de la sección correspondiente a su domicilio puedan votar
    /// </summary>
    Especial,
    /// <summary>
    /// para electores residentes que por condiciones de comunicación o culturales tengan difícil acceso a su casilla
    /// </summary>
    Extraordinaria,
    [Display(Name = "Contigua 11")]
    ContiguaXI,
    [Display(Name = "Contigua 12")]
    ContiguaXII
} // CasillaTipo

/// <summary>
/// Enumeroador para clasificar el propietario de un registro ne la tabla Notas y Archivos
/// </summary>
public enum PropietarioTipo
{
    Ninguno,
    Auditoria,
    Casilla,
    Persona,    
    Seccion,
    Sector    
} // PropietarioTipo

public enum SexoTipo
{
    [Display(Name = "(seleccionar sexo)")]
    Ninguno,
    Masculino,
    Femenino
}

public enum AuditoriaStatusTipo
{
    [Display(Name = "(seleccionar status)")]
    Ninguno,
    Espera,
    Realizada,
    Cancelada,
    Cerrada
}

public enum AuditoriaResultadoTipo
{
    [Display(Name = "(seleccionar resultado)")]
    Ninguno,
    Visitada,
    [Display(Name = "Llamada telefónica")]
    Telefono,
    [Display(Name = "No se encontró")]
    NoEncontrado,
    [Display(Name = "No es el domicilio")]
    NoDomicilio,
    [Display(Name = "No vive nadie")]
    NoViveNadie
}

public enum RepresentanteTipo
{
    [Display(Name = "(representante)")]
    Ninguno,
    [Display(Name = "Titular")]
    Principal,
    Suplente
}

public enum VotoHoraTipo
{
    Ninguno,
    [Display(Name = "8 - 10")]
    NueveDiez,
    [Display(Name = "10 - 12")]
    OnceDoce,
    [Display(Name = "12 - 2")]
    UnaDos,
    [Display(Name = "2 - 4")]
    TresCuatro,
    [Display(Name = "4 - 6")]
    CincoSeis
}

public enum PersonasFiltroEspecificoTipo
{
    Ninguno,
    [Display(Name = "Sin sección")]
    SinSeccion,
    [Display(Name = "Sin casilla")]
    SinCasilla,
    [Display(Name = "Sin número INE")]
    SinNumeroINE
}

/// <summary>
/// Indica el estado civil que puede ser asignado a una persona en México
/// </summary>
public enum EstadoCivilTipo
{
    [Display(Name = "(seleccionar estado civil)")]
    Ninguno,
    Soltero,
    Casado,
    Divorciado,
    [Display(Name = "Separación en proceso judicial")]
    Complicado,
    Viudo,
    Concubinato
} // EstadoCivilTipo

/// <summary>
/// Indica el tipo de candidato que se esta registrando en el sistema
/// </summary>
public enum CandidatoTipo
{
    [Display(Name = "(seleccionar tipo)")]
    Ninguno,
    Partido,
    [Display(Name = "Coalición")]
    Coalicion,
    Independiente
} // CandidatoTipo

public enum CampannaTipo
{
    [Display(Name = "(seleccionar tipo)")]
    Ninguno,
    [Display(Name = "Presidente Municipal")]
    Municipal,
    [Display(Name = "Diputado Local")]
    DiputadoLocal,
    [Display(Name = "Diputado Federal")]
    DiputadoFederal,
    [Display(Name = "Senador")]
    Senador,
    [Display(Name = "Gobernador")]
    Gobernador
}