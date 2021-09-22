using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using System.Web;

public static class IdentityExtensions
{
    public static Guid GetSectorId(this IIdentity identity)
    {
        // https://stackoverflow.com/questions/28335353/how-to-extend-available-properties-of-user-identity
        var claim = ((ClaimsIdentity)identity).FindFirst("SectorID");
        Guid sectorID = Guid.Empty;

        if (claim != null && !string.IsNullOrEmpty(claim.Value))
        {
            sectorID = new Guid(claim.Value);
        }

        return sectorID; // (claim != null) ? new Guid(claim.Value) : Guid.Empty;
    } // GetSectorId

    public static string GetNombreCompleto(this IIdentity identity)
    {
        var claim = ((ClaimsIdentity)identity).FindFirst("NombreCompleto");

        return (claim != null) ? claim.Value : string.Empty;
    } // GetNombreCompleto
}
