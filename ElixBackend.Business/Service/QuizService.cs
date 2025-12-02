using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;

namespace ElixBackend.Business.Service;

public class QuizService(
    IAnswerService answerService,
    IQuestionService questionService, 
    IUserAnswerService userAnswerService) : IQuizService
{
    public async Task<QuizDto?> StartQuizAsync(int userId, int categoryId)
    {
        // Récupérer toutes les questions de la catégorie (avec leurs réponses)
        var categoryQuestions = await questionService.GetQuestionsByCategoryIdAsync(categoryId);
        var questionDtos = categoryQuestions.ToList();
        
        if (!questionDtos.Any())
        {
            return null;
        }
        
        // Grouper les réponses de l'utilisateur par question
        var userAnswersByQuestion = new Dictionary<int, List<UserAnswerDto>>();
        
        foreach (var question in questionDtos)
        {
            var userAnswers = await userAnswerService.GetUserAnswerByUserIdAsync(userId, question.Id);
            var userAnswersList = userAnswers.Where(ua => ua != null).Select(ua => ua!).ToList();
            
            if (userAnswersList.Any())
            {
                userAnswersByQuestion[question.Id] = userAnswersList;
            }
        }

        // Classifier les questions
        var notAnsweredQuestions = new List<QuestionDto>();
        var incorrectlyAnsweredQuestions = new List<QuestionDto>();
        var correctlyAnsweredQuestions = new List<QuestionDto>();

        foreach (var question in questionDtos)
        {
            if (!userAnswersByQuestion.ContainsKey(question.Id))
            {
                notAnsweredQuestions.Add(question);
            }
            else
            {
                var userAnswers = userAnswersByQuestion[question.Id];
                var hasIncorrectAnswer = userAnswers.Any(ua => !ua.IsCorrect);
                
                if (hasIncorrectAnswer)
                {
                    incorrectlyAnsweredQuestions.Add(question);
                }
                else
                {
                    correctlyAnsweredQuestions.Add(question);
                }
            }
        }

        // Sélectionner jusqu'à 10 questions (priorité : non répondues > incorrectes > correctes)
        var selectedQuestions = new List<QuestionDto>();
        
        selectedQuestions.AddRange(notAnsweredQuestions.Take(10));
        
        if (selectedQuestions.Count < 10)
        {
            var remaining = 10 - selectedQuestions.Count;
            selectedQuestions.AddRange(incorrectlyAnsweredQuestions.Take(remaining));
        }
        
        if (selectedQuestions.Count < 10)
        {
            var remaining = 10 - selectedQuestions.Count;
            selectedQuestions.AddRange(correctlyAnsweredQuestions.Take(remaining));
        }

        if (!selectedQuestions.Any())
        {
            return null; 
        }

        // Les réponses sont déjà incluses dans les QuestionDto
        var quizDto = new QuizDto
        {
            Title = $"Quiz - Catégorie {categoryId}",
            CategoryId = categoryId,
            Questions = selectedQuestions
        };

        return quizDto;
    }
}