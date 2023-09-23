﻿using Microsoft.AspNetCore.Components;
using Serilog;
using SwashbucklerDiary.Components;
using SwashbucklerDiary.IServices;
using SwashbucklerDiary.Models;
using SwashbucklerDiary.Utilities;

namespace SwashbucklerDiary.Pages
{
    public partial class IndexMine : ImportantComponentBase
    {
        private int DiaryCount;
        private long WordCount;
        private int ActiveDayCount;
        private string? Language;
        private ThemeState ThemeState;
        private string? UserName;
        private string? Sign;
        private string? Avatar;
        private bool ShowLanguage;
        private bool ShowThemeState;
        private bool ShowFeedback;
        private bool ShowPreviewImage;
        private Dictionary<string, string> Languages => I18n.Languages;
        private readonly static Dictionary<string, ThemeState> ThemeStates = new()
        {
            {"ThemeState.System",ThemeState.System },
            {"ThemeState.Light",ThemeState.Light },
            {"ThemeState.Dark",ThemeState.Dark },
        };
        private Dictionary<string, List<DynamicListItem>> ViewLists = new();
        private List<DynamicListItem> FeedbackTypes = new();
        private Dictionary<string,string> FeedbackTypeDatas = new();

        [Inject]
        private IDiaryService DiaryService { get; set; } = default!;

        protected override void OnInitialized()
        {
            LoadView();
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadViewAsync();
            await SetCount();
            await LoadSettings();
            await SetAvatar();
            await base.OnInitializedAsync();
        }

        protected override async void OnResume()
        {
            await SetCount();
            await LoadSettings();
            await SetAvatar();
            base.OnResume();
        }

        private string MRadioColor => ThemeService.Dark ? "white" : "black";

        private async Task SetCount()
        {
            DiaryCount = await DiaryService.CountAsync(it => !it.Private);
            var wordCountType = (WordCountType)Enum.Parse(typeof(WordCountType), I18n.T("Write.WordCountType")!);
            WordCount = await DiaryService.GetWordCount(wordCountType);
            var diaries = await DiaryService.QueryAsync(it => !it.Private);
            ActiveDayCount = diaries.Select(it => DateOnly.FromDateTime(it.CreateTime)).Distinct().Count();
        }

        private void LoadView()
        {
            ViewLists = new()
            {
                {
                    "Mine.Data",
                    new()
                    {
                        new(this, "Mine.Backups","mdi-folder-sync-outline",() => To("backups")),
                        new(this, "Mine.Export","mdi-export",() => To("export")),
                        new(this, "Mine.Achievement.Name","mdi-trophy-outline",() => To("achievement")),
                    }
                },
                {
                    "Mine.Settings",
                    new()
                    {
                        new(this,"Mine.Settings","mdi-cog-outline",() => To("setting")),
                        new(this,"Mine.Languages","mdi-web",() => ShowLanguage = true),
                        new(this,"Mine.Night","mdi-weather-night",() => ShowThemeState = true),
                    }
                },
                {
                    "Mine.Other",
                    new()
                    {
                        new(this,"Mine.Feedback","mdi-email-outline",() => ShowFeedback = true),
                        new(this,"Mine.About","mdi-information-outline",() => To("about")),
                    }
                }
            };
            FeedbackTypes = new()
            {
                new(this, "Email","mdi-email-outline",SendMail),
                new(this, "Github","mdi-github",ToGithub),
                new(this, "QQGroup","mdi-qqchat",OpenQQGroup),
            };
        }

        private async Task LoadViewAsync()
        {
            FeedbackTypeDatas = await PlatformService.ReadJsonFileAsync<Dictionary<string,string>>("wwwroot/json/feedback-type/feedback-type.json");
        }

        private async Task LoadSettings()
        {
            Language = await SettingsService.Get(SettingType.Language);
            UserName = await SettingsService.Get(SettingType.UserName, I18n.T("AppName"));
            Sign = await SettingsService.Get(SettingType.Sign, I18n.T("Mine.Sign"));
            int themeState = await SettingsService.Get(SettingType.ThemeState);
            ThemeState = (ThemeState)themeState;
        }

        private async Task LanguageChanged(string value)
        {
            Language = value;
            I18n.SetCulture(value);
            await SettingsService.Save(SettingType.Language, Language);
            await SetCount();
        }

        private async Task SetAvatar()
        {
            string uri = await SettingsService.Get(SettingType.Avatar);
            Avatar = StaticCustomScheme.CustomSchemeRender(uri);
        }

        private async Task SendMail()
        {
            var mail = FeedbackTypeDatas["Email"];
            try
            {
                if (PlatformService.IsMailSupported())
                {
                    List<string> recipients = new() { mail };

                    await PlatformService.SendEmail(recipients);
                }
                else
                {
                    await PlatformService.SetClipboard(mail);
                    await AlertService.Success(I18n.T("Mine.MailCopy"));
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e.StackTrace}");
                await AlertService.Error(I18n.T("Mine.SendMailFail"));
            }
        }

        private async Task ToGithub()
        {
            var githubUrl = FeedbackTypeDatas["Github"];
            await PlatformService.OpenBrowser(githubUrl);
        }

        private async Task ThemeStateChanged(ThemeState themeState)
        {
            ThemeState = themeState;
            ThemeService.SetThemeState(ThemeState);
            await SettingsService.Save(SettingType.ThemeState, (int)ThemeState);
        }

        private async Task OpenQQGroup()
        {
            var qqGroup = FeedbackTypeDatas["QQGroup"];
            try
            {
                bool flag = await PlatformService.OpenQQGroup();
                if (!flag)
                {
                    await PlatformService.SetClipboard(qqGroup);
                    await AlertService.Success(I18n.T("Mine.QQGroupCopy"));
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e.StackTrace}");
                await AlertService.Error(I18n.T("Mine.QQGroupError"));
            }
        }
        
        private Task Search(string? value)
        {
            To($"searchAppFunction?query={value}");
            return Task.CompletedTask;
        }
    }
}