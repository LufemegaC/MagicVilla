using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.Modelos.Dto
{
    public class NumeroVillaDto
    {
        [Required]
        public int VillaNo { get; set; }
        [Required]
        //Relacion de entidad Villa
        public int VillaId { get; set; }

        public string DestalleEspecial { get; set; }

    }
}
