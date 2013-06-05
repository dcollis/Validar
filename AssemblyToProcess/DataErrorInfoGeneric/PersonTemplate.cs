using System.ComponentModel;
using Validar;

public class PersonTemplate : IDataErrorInfo , INotifyPropertyChanged
{
    IDataErrorInfo validationTemplate;
    public string GivenNames { get; set; }
    public string FamilyName { get; set; }

    public PersonTemplate()
    {
        validationTemplate = new ValidationTemplate<PersonTemplate>(this);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    string IDataErrorInfo.this[string columnName]
    {
        get { return validationTemplate[columnName]; }
    }

    string IDataErrorInfo.Error
    {
        get { return validationTemplate.Error; }
    }
}