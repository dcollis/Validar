using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Validar
{
    public class ValidationTemplate<T> : IDataErrorInfo
    {
        private static readonly IDictionary<string, ValidationAttribute[]> PropertyToValidatorsDictionary = new Dictionary<string, ValidationAttribute[]>();
        private static readonly IDictionary<string, Func<T, object>> PropertyToGetterDictionary = new Dictionary<string, Func<T, object>>();

        private readonly T _target;

        public ValidationTemplate(INotifyPropertyChanged target)
        {
            _target = (T)target;
            var amendedType = target.GetType();
            CollectValidators(amendedType);
            var interfaceType = amendedType.GetInterfaces().FirstOrDefault(arg => arg.Name == string.Format("I{0}Validator", amendedType.Name));
            if (interfaceType != null) CollectValidators(interfaceType);
        }

        static ValidationTemplate()
        {
            var amendedType = typeof(T);
            CollectValidators(amendedType);
            var interfaceType = amendedType.GetInterfaces().FirstOrDefault(arg => arg.Name == string.Format("I{0}Validator", amendedType.Name));
            if (interfaceType != null) CollectValidators(interfaceType);
        }

        public string this[string columnName]
        {
            get { return ValidateProperty(_target, columnName); }
        }

        public string Error
        {
            get
            {
                var allErrors = new StringBuilder();
                foreach (var property in PropertyToValidatorsDictionary.Keys)
                {
                    var error = ValidateProperty(_target, property);
                    if (!string.IsNullOrEmpty(error)) allErrors.AppendLine(error);
                }
                return allErrors.ToString();
            }
        }

        static void CollectValidators(Type type)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                var validators = propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true) as ValidationAttribute[];
                if (validators == null || validators.Length <= 0) continue;

                var indexParam = propertyInfo.GetIndexParameters();
                if (indexParam.Length != 0) continue;
                PropertyToValidatorsDictionary[propertyInfo.Name] = validators;
                var expressionParameter = Expression.Parameter(type, "t");
                var cast = Expression.TypeAs(Expression.Property(expressionParameter, propertyInfo), typeof(object));
                PropertyToGetterDictionary[propertyInfo.Name] = Expression.Lambda(cast, expressionParameter).Compile() as Func<T, object>;
            }
        }

        public static string ValidateProperty(T instance, string propertyName)
        {
            ValidationAttribute[] validators;
            if (PropertyToValidatorsDictionary.TryGetValue(propertyName, out validators))
            {
                var getter = PropertyToGetterDictionary[propertyName];
                var value = getter(instance);
                var errors = validators.Where(v => v.GetValidationResult(value, new ValidationContext(instance, null, null)) != ValidationResult.Success).Select(v => v.FormatErrorMessage(propertyName)).ToArray();
                return errors.Length == 0 ? null : string.Join(Environment.NewLine, errors);
            }
            return null;
        }

        public static bool IsValid(T instance)
        {
            bool isValid = true;
            foreach (var property in PropertyToValidatorsDictionary.Keys)
            {
                isValid &= string.IsNullOrEmpty(ValidateProperty(instance, property));
            }
            return isValid;
        }

        public static bool HasValidationAttribute
        {
            get { return PropertyToValidatorsDictionary.Count > 0; }
        }
    }
}
