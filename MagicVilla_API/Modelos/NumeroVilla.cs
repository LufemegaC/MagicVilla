using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace MagicVilla_API.Modelos
{
    public class NumeroVilla
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)] //Evita que se genere por SQL
        public int VillaNo { get; set; }
        [Required]
        //Relacion de entidad Villa
        public int VillaId { get; set; }
        [ForeignKey("VillaId")]
        public Villa Villa { get; set; }
        public string DestalleEspecial { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }

    }
}
