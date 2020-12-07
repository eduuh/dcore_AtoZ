using FluentValidation;

namespace Application.validators
{
    public static class ValidatorExtension
    {
        public static IRuleBuilder<T,string> Password<T>(this IRuleBuilder<T, string> rulebuilder){
            var option = rulebuilder.NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters").Matches("[A-Z]").WithMessage("Password must contain 1 uppercase letter").Matches("[a-z]").WithMessage("password must contain at least one lower case letter").Matches("[0-9]").WithMessage("password must contains a number").Matches("[^a-zA-Z0-9]").WithMessage("Password must contain non alphanumeric");

            return option;
        }
    }
}