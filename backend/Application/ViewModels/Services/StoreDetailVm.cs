using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Services
{
    public class StoreDetailVm
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public string ContentHtml { get; set; } = "";
        public List<QuestionAnswerVm> Questionnaire { get; set; } = new();
    }
    public record QuestionAnswerVm(string Question, string Answer);
}
