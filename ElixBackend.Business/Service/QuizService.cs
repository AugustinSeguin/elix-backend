using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class QuizService(
    IQuestionService questionService, 
    IUserAnswerService userAnswerService, 
    IUserPointService userPointService,
    ILogger<QuizService> logger) : IQuizService
{
    public async Task<QuizDto?> StartQuizAsync(int userId, int categoryId)
    {
        try
        {
            // Récupérer toutes les questions de la catégorie (avec leurs réponses)
            var categoryQuestions = await questionService.GetQuestionsByCategoryIdAsync(categoryId);
            var questionDtos = categoryQuestions?.ToList() ?? [];

            if (!questionDtos.Any())
            {
                return null;
            }

            // Filtrer les questions : au minimum 2 réponses et 1 réponse valide
            questionDtos = questionDtos.Where(q => 
                q.Answers != null && 
                q.Answers.Count() >= 2 && 
                q.Answers.Any(a => a.IsValid)
            ).ToList();

            if (!questionDtos.Any())
            {
                return null;
            }

            // Grouper les réponses de l'utilisateur par question
            var userAnswersByQuestion = new Dictionary<int, List<UserAnswerDto>>();

            foreach (var question in questionDtos)
            {
                var userAnswers = await userAnswerService.GetUserAnswerByUserIdAsync(userId, question.Id);
                var userAnswersList = (userAnswers).Where(ua => ua != null).Select(ua => ua!).ToList();

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
                if (!userAnswersByQuestion.TryGetValue(question.Id, out var userAnswers))
                {
                    notAnsweredQuestions.Add(question);
                }
                else
                {
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
        catch (Exception ex)
        {
            logger.LogError(ex, "QuizService.StartQuizAsync failed for userId {UserId}, categoryId {CategoryId}", userId, categoryId);
            return null;
        }
    }


    public async Task<List<CorrectionDto>?> SubmitQuizAsync(QuizSubmissionDto quizSubmission)
    {
        try
        {
            var userId = quizSubmission.UserId;
            var categoryId = quizSubmission.CategoryId;
            var correctAnswersCount = 0;
            var totalQuestions = quizSubmission.UserAnswers.Count;

            // Récupérer toutes les questions de la catégorie avec leurs réponses
            var categoryQuestions = await questionService.GetQuestionsByCategoryIdAsync(categoryId);
            var questionsList = categoryQuestions?.ToList() ?? [];

            if (!questionsList.Any())
            {
                return null;
            }

            var corrections = new List<CorrectionDto>();

            // Traiter chaque réponse de l'utilisateur
            foreach (var userAnswer in quizSubmission.UserAnswers)
            {
                var question = questionsList.FirstOrDefault(q => q.Id == userAnswer.QuestionId);
                
                if (question?.Answers == null || !question.Answers.Any())
                {
                    continue;
                }

                // Trouver la réponse correcte pour cette question
                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsValid);
                
                if (correctAnswer == null)
                {
                    continue;
                }

                // Vérifier si l'utilisateur a donné la bonne réponse
                var isCorrect = userAnswer.AnswerIdSelected == correctAnswer.Id;

                if (isCorrect)
                {
                    correctAnswersCount++;
                }

                // Enregistrer la réponse de l'utilisateur dans la base de données
                var userAnswerEntity = new UserAnswerDto
                {
                    UserId = userId,
                    QuestionId = question.Id,
                    IsCorrect = isCorrect
                };

                await userAnswerService.AddUserAsync(userAnswerEntity);

                // Créer l'objet CorrectionDto
                var correction = new CorrectionDto
                {
                    QuestionId = question.Id,
                    Question = question,
                    SelectedAnswerId = userAnswer.AnswerIdSelected,
                    IsCorrect = isCorrect,
                    Explanation = correctAnswer.Explanation,
                    Answer = correctAnswer
                };
                corrections.Add(correction);
            }

            // Si l'utilisateur a au moins 8 bonnes réponses sur 10, il gagne des points
            if (totalQuestions >= 10 && correctAnswersCount >= 8)
            {
                var userPointDto = new UserPointDto
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Points = correctAnswersCount
                };

                await userPointService.AddUserPointAsync(userPointDto);
            }

            return corrections;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuizService.SubmitQuizAsync failed for userId {UserId}, categoryId {CategoryId}", quizSubmission.UserId, quizSubmission.CategoryId);
            return null;
        }
    }
}
