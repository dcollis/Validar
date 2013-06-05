using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Validar;

[InjectValidation]
public class Person : INotifyPropertyChanged
{
    [Required(ErrorMessage = "Required")]
    public string GivenNames { get; set; }
    [Required(ErrorMessage = "Required")]
    public string FamilyName { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}