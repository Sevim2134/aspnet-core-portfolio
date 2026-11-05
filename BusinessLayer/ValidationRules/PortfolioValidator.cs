using EntityLayer.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.ValidationRules
{
    public class PortfolioValidator:AbstractValidator<Portfolio>
    {
        public PortfolioValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Proje adı boş geçilemez.");
            RuleFor(x => x.ImgUrl).NotEmpty().WithMessage("Görsel alanı boş geçilemez");
            RuleFor(x => x.ImageUrl2).NotEmpty().WithMessage("Görsel2 alanı boş geçilemez");
            RuleFor(x => x.Price).NotEmpty().WithMessage("Fiyat alanı boş geçilemez.");
            RuleFor(x => x.Value).NotEmpty().WithMessage("Tamamlanma oranı  alanı boş geçilemez.");
            RuleFor(x => x.Name).MinimumLength(5).WithMessage("Proje ismi en az 5 karekterden oluşmak zorundadır.");
            RuleFor(x => x.Name).MaximumLength(100).WithMessage("Proje adı 100 karekteri geçmemeli.");

        }
    }
}
