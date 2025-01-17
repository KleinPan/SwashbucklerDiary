﻿using Microsoft.AspNetCore.Components;
using Serilog;
using SwashbucklerDiary.IServices;
using SwashbucklerDiary.Models;

namespace SwashbucklerDiary.Components
{
    public partial class ExportDialog : DialogComponentBase
    {
        private List<DynamicListItem> Items = new();

        [Inject]
        protected IPlatformService PlatformService { get; set; } = default!;

        [Inject]
        protected IAppDataService AppDataService { get; set; } = default!;

        [Parameter]
        public List<DiaryModel> Diaries { get; set; } = new();

        protected override void OnInitialized()
        {
            LoadView();
            base.OnInitialized();
        }

        private void LoadView()
        {
            Items = new()
            {
                new(this,"TXT", "mdi-format-text", CreateTxtFile),
                new(this,"MD", "mdi-language-markdown-outline", CreateMdFile),
                new(this,"JSON", "mdi-code-braces", CreateJsonFile),
                new(this,"XLSX", "mdi-table-large", CreateXlsxFile),
            };
        }

        private Task CreateTxtFile() => CreateFile(AppDataService.ExportTxtZipFileAndSaveAsync);

        private Task CreateJsonFile() => CreateFile(AppDataService.ExportJsonZipFileAndSaveAsync);

        private Task CreateMdFile() => CreateFile(AppDataService.ExportMdZipFileAndSaveAsync);

        private Task CreateXlsxFile() => CreateFile(AppDataService.ExportXlsxFileAndSaveAsync);

        private async Task CreateFile(Func<List<DiaryModel>, Task<bool>> func)
        {
            await InternalValueChanged(false);
            await AlertService.StartLoading();
            _ = Task.Run(async () =>
            {
                try
                {
                    bool flag = await func(Diaries);
                    if (flag)
                    {
                        await AlertService.Success(I18n.T("Export.Export.Success"));
                        await HandleAchievements(AchievementType.Export);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"{e.Message}\n{e.StackTrace}");
                    await AlertService.Error(I18n.T("Export.Export.Fail"));
                }
                finally
                {
                    await AlertService.StopLoading();
                }
            });

        }
    }
}
