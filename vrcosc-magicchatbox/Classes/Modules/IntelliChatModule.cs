﻿using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Moderations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using vrcosc_magicchatbox.ViewModels;

namespace vrcosc_magicchatbox.Classes.Modules
{
    public partial class IntelliChatModuleSettings : ObservableObject
    {

        [ObservableProperty]
        private bool autolanguageSelection = true;

        [ObservableProperty]
        private bool intelliChatError = false;

        [ObservableProperty]
        private string intelliChatErrorTxt = string.Empty;

        [ObservableProperty]
        private bool intelliChatPerformModeration = true;

        [ObservableProperty]
        private int intelliChatPerformModerationTimeout = 7;

        [ObservableProperty]
        private int intelliChatTimeout = 10;

        [ObservableProperty]
        private string intelliChatTxt = string.Empty;

        [ObservableProperty]
        private bool intelliChatUILabel = false;

        [ObservableProperty]
        private string intelliChatUILabelTxt = string.Empty;

        [ObservableProperty]
        private bool intelliChatWaitingToAccept = false;

        [ObservableProperty]
        private List<SupportedIntelliChatLanguage> selectedSupportedLanguages = new List<SupportedIntelliChatLanguage>();

        [ObservableProperty]
        private SupportedIntelliChatLanguage selectedTranslateLanguage;

        [ObservableProperty]
        private IntelliChatWritingStyle selectedWritingStyle;
        [ObservableProperty]
        private List<SupportedIntelliChatLanguage> supportedLanguages = new List<SupportedIntelliChatLanguage>();

        [ObservableProperty]
        private List<IntelliChatWritingStyle> supportedWritingStyles = new List<IntelliChatWritingStyle>();
    }

    public partial class SupportedIntelliChatLanguage : ObservableObject
    {
        [ObservableProperty]
        private int iD;

        [ObservableProperty]
        private bool isBuiltIn = false;

        [ObservableProperty]
        private string language;


        [ObservableProperty]
        private bool isFavorite = false;
    }

    public partial class IntelliChatWritingStyle : ObservableObject
    {
        [ObservableProperty]
        private int iD;

        [ObservableProperty]
        private bool isBuiltIn;

        [ObservableProperty]
        private string styleDescription;

        [ObservableProperty]
        private string styleName;

        [ObservableProperty]
        private double temperature;

        [ObservableProperty]
        private bool isFavorite = false;
    }

    public partial class IntelliChatModule : ObservableObject
    {
        private const string IntelliChatSettingsFileName = "IntelliChatSettings.json";

        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static bool _isInitialized = false;

        [ObservableProperty]
        private IntelliChatModuleSettings settings = new IntelliChatModuleSettings();

        public IntelliChatModule()
        {
            Initialize();
        }

        private bool EnsureInitialized()
        {
            if (!_isInitialized)
            {
                UpdateErrorState(true, "IntelliChat not initialized.");
                return false;
            }
            if (!OpenAIModule.Instance.IsInitialized)
            {
                UpdateErrorState(true, "OpenAI client not initialized.");
                return false;
            }

            return true;
        }

        private bool EnsureInitializedAndNotEmpty(string text)
        {
            if (!EnsureInitialized())
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return true;
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            LoadSettings();
            _isInitialized = true;
        }
        private void InitializeDefaultLanguageSettings()
        {
            var defaultLanguages = new List<SupportedIntelliChatLanguage>
            {
                new SupportedIntelliChatLanguage { ID = 1, Language = "English", IsBuiltIn = true, IsFavorite = true },
                new SupportedIntelliChatLanguage { ID = 2, Language = "Spanish", IsBuiltIn = true, IsFavorite = true },
                new SupportedIntelliChatLanguage { ID = 3, Language = "French", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 4, Language = "German", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 5, Language = "Chinese", IsBuiltIn = true, IsFavorite = true },
                new SupportedIntelliChatLanguage { ID = 6, Language = "Japanese", IsBuiltIn = true, IsFavorite = true },
                new SupportedIntelliChatLanguage { ID = 7, Language = "Russian", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 8, Language = "Portuguese", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 9, Language = "Italian", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 10, Language = "Dutch", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 11, Language = "Arabic", IsBuiltIn = true, IsFavorite = true },
                new SupportedIntelliChatLanguage { ID = 12, Language = "Turkish", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 13, Language = "Korean", IsBuiltIn = true, IsFavorite = true },
                new SupportedIntelliChatLanguage { ID = 14, Language = "Hindi", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 15, Language = "Swedish", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 16, Language = "Norwegian", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 17, Language = "Danish", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 18, Language = "Finnish", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 19, Language = "Greek", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 20, Language = "Hebrew", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 21, Language = "Polish", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 22, Language = "Czech", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 23, Language = "Thai", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 24, Language = "Indonesian", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 25, Language = "Malay", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 26, Language = "Vietnamese", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 27, Language = "Tagalog", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 28, Language = "Bengali", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 29, Language = "Tamil", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 30, Language = "Telugu", IsBuiltIn = true },
                new SupportedIntelliChatLanguage { ID = 31, Language = "Language no one understands", IsBuiltIn = true }
            };

            Settings.SupportedLanguages = defaultLanguages;
        }
        private void InitializeDefaultSettings()
        {
            InitializeDefaultLanguageSettings();
            InitializeDefaultWritingStyleSettings();

            SaveSettings();
        }

        public void EnsureValidSelections()
        {
            // Update the selected writing style based on ID, ensuring it is part of the supported styles list.
            var selectedStyle = Settings.SelectedWritingStyle != null
                ? Settings.SupportedWritingStyles.FirstOrDefault(style => style.ID == Settings.SelectedWritingStyle.ID)
                : null;

            var selectedTranslateLanguage = Settings.SelectedTranslateLanguage != null
                ? Settings.SupportedLanguages.FirstOrDefault(lang => lang.ID == Settings.SelectedTranslateLanguage.ID)
                : null;

            Settings.SelectedWritingStyle = selectedStyle ?? Settings.SupportedWritingStyles.FirstOrDefault(style => style.IsBuiltIn);

            Settings.SelectedTranslateLanguage = selectedTranslateLanguage ?? Settings.SupportedLanguages.Where(lang => lang.Language.Equals("English", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }




        private void InitializeDefaultWritingStyleSettings()
        {
            var defaultStyles = new List<IntelliChatWritingStyle>
    {
        new IntelliChatWritingStyle { ID = 1, StyleName = "Casual", StyleDescription = "A casual, everyday writing style", Temperature = 0.6, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 2, StyleName = "Formal", StyleDescription = "A formal, professional writing style", Temperature = 0.3, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 3, StyleName = "Friendly", StyleDescription = "A friendly, approachable writing style", Temperature = 0.5, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 4, StyleName = "Professional", StyleDescription = "A professional, business-oriented writing style", Temperature = 0.4, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 5, StyleName = "Academic", StyleDescription = "An academic, scholarly writing style", Temperature = 0.3, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 6, StyleName = "Creative", StyleDescription = "A creative, imaginative writing style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 7, StyleName = "Humorous", StyleDescription = "A humorous, funny writing style", Temperature = 0.9, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 8, StyleName = "British", StyleDescription = "A British, UK-specific writing style", Temperature = 0.5, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 9, StyleName = "Sarcastic", StyleDescription = "A sarcastic, witty writing style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 10, StyleName = "Romantic", StyleDescription = "A romantic, lovey-dovey writing style", Temperature = 0.6, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 11, StyleName = "Action-Packed", StyleDescription = "An action-packed, adrenaline-fueled writing style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 12, StyleName = "Mysterious", StyleDescription = "A mysterious, suspenseful writing style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 13, StyleName = "Sci-Fi", StyleDescription = "A futuristic, sci-fi writing style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 14, StyleName = "Horror", StyleDescription = "A chilling, horror writing style", Temperature = 0.85, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 15, StyleName = "Western", StyleDescription = "A wild west, cowboy writing style", Temperature = 0.6, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 16, StyleName = "Fantasy", StyleDescription = "A magical, fantasy writing style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 17, StyleName = "Superhero", StyleDescription = "A heroic, superhero writing style", Temperature = 0.65, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 18, StyleName = "Film Noir", StyleDescription = "A dark, film noir writing style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 19, StyleName = "Comedy", StyleDescription = "A hilarious, comedy writing style", Temperature = 0.9, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 20, StyleName = "Drama", StyleDescription = "A dramatic, tear-jerking writing style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 21, StyleName = "Risqué Humor", StyleDescription = "A bold, cheeky humor style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 22, StyleName = "Flirty Banter", StyleDescription = "A playful, flirtatious banter style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 23, StyleName = "Sensual Poetry", StyleDescription = "A deeply sensual, poetic style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 24, StyleName = "Bold Confessions", StyleDescription = "A daring, confessional narrative style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 25, StyleName = "Seductive Fantasy", StyleDescription = "A seductive, enchanting fantasy style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 26, StyleName = "Candid Chronicles", StyleDescription = "A frank, revealing chronicle style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 27, StyleName = "Playful Teasing", StyleDescription = "A light-hearted, teasing narrative style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 28, StyleName = "Intimate Reflections", StyleDescription = "A deeply personal, intimate reflection style", Temperature = 0.65, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 29, StyleName = "Enigmatic Erotica", StyleDescription = "A mysterious, subtly erotic style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 30, StyleName = "Whimsical Winks", StyleDescription = "A whimsical, suggestive wink style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 31, StyleName = "Lavish Desires", StyleDescription = "A richly descriptive, desire-filled style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 32, StyleName = "Cheeky Charm", StyleDescription = "A cheeky, charmingly persuasive style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 33, StyleName = "Elegant Enticement", StyleDescription = "An elegantly enticing, sophisticated style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 34, StyleName = "Veiled Allusions", StyleDescription = "A subtly allusive, veiled narrative style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 35, StyleName = "Rousing Revelations", StyleDescription = "A stimulating, revelation-rich style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 36, StyleName = "Tantalizing Tales", StyleDescription = "A tantalizing, teasing tale style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 37, StyleName = "Forbidden Fantasies", StyleDescription = "A style rich with forbidden fantasies", Temperature = 0.85, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 38, StyleName = "Wicked Whispers", StyleDescription = "A wickedly whispering, secretive style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 39, StyleName = "Coquettish Confidences", StyleDescription = "A coquettish, confidently playful style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 40, StyleName = "Thriller", StyleDescription = "A thrilling, suspenseful writing style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 41, StyleName = "Noir Romance", StyleDescription = "A sultry, mysterious romance style", Temperature = 0.7, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 42, StyleName = "Sensual Noir", StyleDescription = "A dark, sensual noir style", Temperature = 0.8, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 43, StyleName = "Seductive Noir", StyleDescription = "A seductive, mysterious noir style", Temperature = 0.75, IsBuiltIn = true },
        new IntelliChatWritingStyle { ID = 44, StyleName = "Sultry Noir", StyleDescription = "A sultry, mysterious noir style", Temperature = 0.7, IsBuiltIn = true }
    };

            Settings.SupportedWritingStyles = defaultStyles;
        }

        private void ProcessError(Exception ex)
        {

            if (ex is OperationCanceledException)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    // This block is entered either if the operation was manually cancelled
                    // or if it was cancelled due to a timeout.
                    // You might want to check if the cancellation was due to a timeout:
                    if (_cancellationTokenSource.Token.WaitHandle.WaitOne(0))
                    {
                        // Handle the timeout-specific logic here
                        UpdateErrorState(true, "The operation was cancelled due to a timeout.");
                        return;
                    }
                    return;

                }
            }

            Settings.IntelliChatUILabel = false;
            Settings.IntelliChatUILabelTxt = string.Empty;

            UpdateErrorState(true, ex.Message);
        }

        public void CloseIntelliErrorPanel()
        {
            Settings.IntelliChatError = false;
            Settings.IntelliChatErrorTxt = string.Empty;
        }

        private void ProcessResponse(ChatResponse? response)
        {
            if (response?.Choices?[0].Message.Content.ValueKind == JsonValueKind.String)
            {
                Settings.IntelliChatUILabel = false;
                Settings.IntelliChatUILabelTxt = string.Empty;

                Settings.IntelliChatTxt = response.Choices[0].Message.Content.GetString();
                Settings.IntelliChatWaitingToAccept = true;

            }
            else
            {
                Settings.IntelliChatUILabel = false;
                Settings.IntelliChatUILabelTxt = string.Empty;

                Settings.IntelliChatTxt = response?.Choices?[0].Message.Content.ToString() ?? string.Empty;
                Settings.IntelliChatWaitingToAccept = true;
            }
        }

        private void UpdateErrorState(bool hasError, string errorMessage)
        {
            Settings.IntelliChatError = hasError;
            Settings.IntelliChatErrorTxt = errorMessage;
        }

        public void AcceptIntelliChatSuggestion()
        {
            ViewModel.Instance.NewChattingTxt = Settings.IntelliChatTxt;
            Settings.IntelliChatTxt = string.Empty;
            Settings.IntelliChatWaitingToAccept = false;


        }
        public void AddWritingStyle(string styleName, string styleDescription, double temperature)
        {
            // Find the next available ID starting from 1000 for user-defined styles
            int nextId = Settings.SupportedWritingStyles.DefaultIfEmpty().Max(style => style?.ID ?? 999) + 1;
            if (nextId < 1000) nextId = 1000; // Ensure starting from 1000

            // Check if the style name already exists
            if (Settings.SupportedWritingStyles.Any(style => style.StyleName.Equals(styleName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A writing style with the name \"{styleName}\" already exists.");
            }

            // Add the new writing style
            var newStyle = new IntelliChatWritingStyle
            {
                ID = nextId,
                StyleName = styleName,
                StyleDescription = styleDescription,
                Temperature = temperature,
                IsBuiltIn = false, // User-defined styles are not built-in
            };
            Settings.SupportedWritingStyles.Add(newStyle);

            SaveSettings(); // Save the updated settings
        }

        public void CancelAllCurrentTasks()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }



        public async Task GenerateConversationStarterAsync()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            try
            {

                Settings.IntelliChatUILabel = true;
                Settings.IntelliChatUILabelTxt = "Waiting for OpenAI to respond";

                var prompt = "Please generate a short a creative and engaging conversation starter of max 140 characters (this includes spaces), avoid AI and tech";

                ResetCancellationToken(Settings.IntelliChatTimeout);

                var response = await OpenAIModule.Instance.OpenAIClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(new List<Message> { new Message(Role.System, prompt) }, maxTokens: 60), _cancellationTokenSource.Token);

                if (response == null)
                {
                    throw new InvalidOperationException("The response from OpenAI was empty");
                }
                else
                {
                    ProcessResponse(response);
                }
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }

        }
        public void LoadSettings()
        {
            var filePath = Path.Combine(ViewModel.Instance.DataPath, IntelliChatSettingsFileName);
            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                Settings = JsonConvert.DeserializeObject<IntelliChatModuleSettings>(jsonData) ?? new IntelliChatModuleSettings();
            }
            else
            {
                InitializeDefaultSettings();
            }
            EnsureValidSelections();
        }

        public async Task<bool> ModerationCheckPassedAsync(string text, bool cancelAllTasks = true)
        {
            if (cancelAllTasks)
                CancelAllCurrentTasks();

            if (!Settings.IntelliChatPerformModeration) return true;

            Settings.IntelliChatUILabel = true;
            Settings.IntelliChatUILabelTxt = "performing moderation check";

            try
            {
                ResetCancellationToken(Settings.IntelliChatPerformModerationTimeout);

                var moderationResponse = await OpenAIModule.Instance.OpenAIClient.ModerationsEndpoint.CreateModerationAsync(new ModerationsRequest(text), _cancellationTokenSource.Token);

                if (moderationResponse?.Results.Any(r => r.Flagged) ?? false)
                {
                    Settings.IntelliChatUILabel = false;

                    UpdateErrorState(true, "Your message has been temporarily held back due to a moderation check.\nThis is to ensure compliance with OpenAI's guidelines and protect your account.");
                    return false;
                }
                else
                {
                    Settings.IntelliChatUILabel = false;

                    UpdateErrorState(false, string.Empty);
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                Settings.IntelliChatUILabel = false;

                UpdateErrorState(false, string.Empty);
                return true;
            }
        }


        public async Task PerformBeautifySentenceAsync(string text, IntelliChatWritingStyle intelliChatWritingStyle = null)
        {
            if (!OpenAIModule.Instance.IsInitialized)
            {
                ViewModel.Instance.ActivateSetting("Settings_OpenAI");
            }

            if (!EnsureInitializedAndNotEmpty(text))
            {
                return;
            }

            try
            {
                if (!await ModerationCheckPassedAsync(text)) return;

                Settings.IntelliChatUILabel = true;
                Settings.IntelliChatUILabelTxt = "Waiting for OpenAI to respond";

                intelliChatWritingStyle = intelliChatWritingStyle ?? Settings.SelectedWritingStyle;

                var messages = new List<Message>
                {
                    new Message(Role.System, $"Please rewrite the following sentence in {intelliChatWritingStyle.StyleDescription} style:")
                };

                if (!Settings.AutolanguageSelection && Settings.SelectedSupportedLanguages.Count > 0)
                {
                    // Extracting the Language property from each SupportedIntelliChatLanguage object
                    var languages = Settings.SelectedSupportedLanguages.Select(lang => lang.Language).ToList();

                    // Joining the language strings with commas
                    var languagesString = string.Join(", ", languages);

                    messages.Add(new Message(Role.System, $"Consider these languages: {languagesString}"));
                }


                messages.Add(new Message(Role.User, text));

                ResetCancellationToken(Settings.IntelliChatTimeout);

                var response = await OpenAIModule.Instance.OpenAIClient.ChatEndpoint
                    .GetCompletionAsync(new ChatRequest(messages: messages, maxTokens: 120, temperature: intelliChatWritingStyle.Temperature), _cancellationTokenSource.Token);

                if (response == null)
                {
                    throw new InvalidOperationException("The response from OpenAI was empty");
                }

                ProcessResponse(response);
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }



        }



        public async Task PerformLanguageTranslationAsync(string text, SupportedIntelliChatLanguage supportedIntelliChatLanguage = null)
        {
            if (!EnsureInitializedAndNotEmpty(text))
            {
                return;
            }

            try
            {
                if (!await ModerationCheckPassedAsync(text)) return;

                SupportedIntelliChatLanguage intelliChatLanguage = supportedIntelliChatLanguage ?? settings.SelectedTranslateLanguage;

                var messages = new List<Message>
                {
                    new Message(Role.System, $"Translate this to {intelliChatLanguage.Language}:"),
                    new Message(Role.User, text)
                };

                Settings.IntelliChatUILabel = true;
                Settings.IntelliChatUILabelTxt = "Waiting for OpenAI to respond";

                ResetCancellationToken(Settings.IntelliChatTimeout);

                var response = await OpenAIModule.Instance.OpenAIClient.ChatEndpoint
                    .GetCompletionAsync(new ChatRequest(messages: messages, maxTokens: 120), _cancellationTokenSource.Token);

                if (response == null)
                {
                    Settings.IntelliChatUILabel = false;
                    throw new InvalidOperationException("The response from OpenAI was empty");
                }
                else
                {
                    Settings.IntelliChatUILabel = false;
                    ProcessResponse(response);
                }

            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }


        }


        public async Task PerformSpellingAndGrammarCheckAsync(string text)
        {
            if (!OpenAIModule.Instance.IsInitialized)
            {
                ViewModel.Instance.ActivateSetting("Settings_OpenAI");
            }
            if (!EnsureInitializedAndNotEmpty(text))
            {
                return;
            }

            try
            {
                if (!await ModerationCheckPassedAsync(text)) return;

                Settings.IntelliChatUILabel = true;
                Settings.IntelliChatUILabelTxt = "Waiting for OpenAI to respond";

                var messages = new List<Message>
                {
                new Message(
                    Role.System,
                    "Please detect and correct and return any spelling and grammar errors in the following text:")
                };

                if (!Settings.AutolanguageSelection && Settings.SelectedSupportedLanguages.Count > 0)
                {
                    // Extracting the Language property from each SupportedIntelliChatLanguage object
                    var languages = Settings.SelectedSupportedLanguages.Select(lang => lang.Language).ToList();

                    // Joining the language strings with commas
                    var languagesString = string.Join(", ", languages);

                    messages.Add(new Message(Role.System, $"Consider these languages: {languagesString}"));
                }


                messages.Add(new Message(Role.User, text));

                ResetCancellationToken(Settings.IntelliChatTimeout);

                ChatResponse response = await OpenAIModule.Instance.OpenAIClient.ChatEndpoint
                    .GetCompletionAsync(new ChatRequest(messages: messages, maxTokens: 120), _cancellationTokenSource.Token);

                if (response == null)
                {
                    throw new InvalidOperationException("The response from OpenAI was empty");
                }
                else
                {
                    ProcessResponse(response);
                }
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }


        }

        public void RejectIntelliChatSuggestion()
        {
            Settings.IntelliChatTxt = string.Empty;
            Settings.IntelliChatWaitingToAccept = false;
        }
        public void RemoveWritingStyle(int styleId)
        {
            var styleToRemove = Settings.SupportedWritingStyles.FirstOrDefault(style => style.ID == styleId);
            if (styleToRemove == null)
            {
                throw new InvalidOperationException($"No writing style found with ID {styleId}.");
            }

            if (styleToRemove.IsBuiltIn)
            {
                throw new InvalidOperationException("Built-in writing styles cannot be removed.");
            }

            Settings.SupportedWritingStyles.Remove(styleToRemove);
            SaveSettings(); // Save the updated settings
        }

        public void ResetCancellationToken(int timeoutInSeconds)
        {
            CancelAllCurrentTasks();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.CancelAfter(timeoutInSeconds * 1000);
        }
        public void SaveSettings()
        {
            var filePath = Path.Combine(ViewModel.Instance.DataPath, IntelliChatSettingsFileName);
            var jsonData = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }

        public async Task ShortenTextAsync(string text, int retryCount = 0)
        {
            if (!EnsureInitializedAndNotEmpty(text))
            {
                return;
            }

            try
            {
                if (!await ModerationCheckPassedAsync(text)) return;

                Settings.IntelliChatUILabel = true;
                Settings.IntelliChatUILabelTxt = "Waiting for OpenAI to respond";

                string prompt = retryCount == 0
                ? $"Shorten ONLY the following text to 140 characters or less dont add anything, including spaces: {text}"
                : $"Please be more concise. Shorten ONLY this text to 140 characters or less don't add more into it, including spaces: {text}";

                ResetCancellationToken(Settings.IntelliChatTimeout);

                var response = await OpenAIModule.Instance.OpenAIClient.ChatEndpoint
                    .GetCompletionAsync(new ChatRequest(new List<Message>
                    {
            new Message(Role.System, prompt)
                    }, maxTokens: 60), _cancellationTokenSource.Token);

                var shortenedText = response?.Choices?[0].Message.Content.ValueKind == JsonValueKind.String
                    ? response.Choices[0].Message.Content.GetString()
                    : string.Empty;

                // Check if the response is still over 140 characters and retry if necessary
                if (shortenedText.Length > 140 && retryCount < 2) // Limiting to one retry
                {
                    await ShortenTextAsync(shortenedText, retryCount + 1);
                }
                else
                {
                    ProcessResponse(response);
                }
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }


        }

        public async Task GenerateCompletionOrPredictionAsync(string inputText, bool isNextWordPrediction = false)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                UpdateErrorState(true, "Input text cannot be empty.");
                return;
            }

            if (!await ModerationCheckPassedAsync(inputText))
            {
                return;
            }

            try
            {
                Settings.IntelliChatUILabel = true;
                Settings.IntelliChatUILabelTxt = isNextWordPrediction ? "Predicting next word..." : "Generating completion...";

                var promptMessage = isNextWordPrediction ? "Predict the next word." : "Complete the following text.";
                var messages = new List<Message>
            {
                new Message(Role.System, promptMessage),
                new Message(Role.User, inputText)
            };

                // Customizing ChatRequest for the task
                var chatRequest = new ChatRequest(
                    messages: messages,
                    model: "gpt-3.5-turbo", // Fast and efficient model
                    maxTokens: isNextWordPrediction ? 1 : 50, // Adjust based on the task
                    temperature: 0.7, // Fine-tune for creativity vs. randomness
                    topP: 1,
                    frequencyPenalty: 0.5, // Adjust as needed
                    presencePenalty: 0.5 // Adjust as needed
                );

                var response = await OpenAIModule.Instance.OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);

               if (response == null)
                {
                    throw new InvalidOperationException("The response from OpenAI was empty");
                }
                else
                {
                    ProcessResponse(response);
                }
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }
        }


    }
}
