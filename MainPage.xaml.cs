using System.Text.Json;

namespace QuizApp;

public partial class MainPage : ContentPage
{
    private List<Question> questions = new();
    private int currentQuestion = 0;
    private int score = 0;

    private const string Background1 = "player1_bg.png";
    private const string Background2 = "player2_bg.png";

    public MainPage()
    {
        InitializeComponent();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadQuestionsAsync();

        if (questions == null || questions.Count == 0)
        {
            await DisplayAlert("Błąd", "Plik pytania.json jest pusty lub nieprawidłowy.", "OK");
            return;
        }

        currentQuestion = 0;
        score = 0;

        BackgroundImage.Source = Background1;
        LoadQuestion();
    }

    private async Task LoadQuestionsAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("pytania.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            // JSON ma strukturę { "quiz": [ ... ] }
            var quizWrapper = JsonSerializer.Deserialize<QuizWrapper>(json);
            questions = quizWrapper?.quiz ?? new List<Question>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się wczytać pytań: {ex.Message}", "OK");
            questions = new();
        }
    }

    private void LoadQuestion()
    {
        if (questions.Count == 0 || currentQuestion >= questions.Count)
            return;

        // zmiana tła po 5 pytaniach
        BackgroundImage.Source = currentQuestion >= 5 ? Background2 : Background1;

        var q = questions[currentQuestion];

        QuestionLabel.Text = q.question;
        BtnA.Text = q.answers[0];
        BtnB.Text = q.answers[1];
        BtnC.Text = q.answers[2];
        BtnD.Text = q.answers[3];
    }

    private void AnswerClicked(object sender, EventArgs e)
    {
        if (questions.Count == 0 || currentQuestion >= questions.Count)
            return;

        Button btn = (Button)sender;
        var q = questions[currentQuestion];

        // sprawdzanie odpowiedzi po tekście
        if (btn.Text == q.answers[q.correct])
            score++;

        ScoreLabel.Text = $"Punkty: {score}";
    }

    private void NextClicked(object sender, EventArgs e)
    {
        currentQuestion++;

        if (currentQuestion < questions.Count)
            LoadQuestion();
        else
            DisplayAlert("Koniec quizu", $"Twój wynik: {score}/{questions.Count}", "OK");
    }
}

// klasa do deserializacji głównego JSON
public class QuizWrapper
{
    public List<Question> quiz { get; set; } = new();
}

// pojedyncze pytanie
public class Question
{
    public string question { get; set; } = "";
    public List<string> answers { get; set; } = new();
    public int correct { get; set; }
}
