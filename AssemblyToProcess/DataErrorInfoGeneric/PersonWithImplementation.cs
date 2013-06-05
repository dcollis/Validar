using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Validar;

[InjectValidation]
public class PersonWithImplementation : INotifyPropertyChanged, IDataErrorInfo
{
    IDataErrorInfo validationTemplate;
    public PersonWithImplementation()
    {
        validationTemplate = new ValidationTemplate<PersonWithImplementation>(this);
    }

    public string this[string columnName]
    {
        get { return validationTemplate[columnName]; }
    }

    public string Error
    {
        get { return validationTemplate.Error; }
    }

    string givenNames;

    [Required(ErrorMessage = "Required")]
    public string GivenNames
    {
        get { return givenNames; }
        set
        {
            if (value != givenNames)
            {
                givenNames = value;
                OnPropertyChanged("GivenNames");
            }
        }
    }

    string familyName;

    [Required(ErrorMessage = "Required")]
    public string FamilyName
    {
        get { return familyName; }
        set
        {
            if (value != familyName)
            {
                familyName = value;
                OnPropertyChanged("FamilyName");
            }
        }
    }

    public virtual void OnPropertyChanged(string propertyName)
    {
        var propertyChanged = PropertyChanged;
        if (propertyChanged != null)
        {
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}