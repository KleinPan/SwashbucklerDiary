﻿using Microsoft.AspNetCore.Components;
using Serilog;
using SwashbucklerDiary.Components;
using SwashbucklerDiary.IServices;
using SwashbucklerDiary.Models;

namespace SwashbucklerDiary.Pages
{
    public partial class LocalExport : ImportantComponentBase
    {
        private bool ShowExport;

        private bool ShowImport;

        private string? ImportFilePath;

        private List<DiaryModel> Diaries = new();

        [Inject]
        private IDiaryService DiaryService { get; set; } = default!;

        [Inject]
        private IAppDataService AppDataService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Diaries = await DiaryService.QueryAsync();
            await base.OnInitializedAsync();
        }

        private async Task Export()
        {
            var flag = await PlatformService.TryStorageWritePermission();
            if (!flag)
            {
                return;
            }

            if (!Diaries.Any())
            {
                await AlertService.Info(I18n.T("Diary.NoDiary"));
                return;
            }

            ShowExport = true;
        }

        private async Task Import()
        {
            var flag = await PlatformService.TryStorageWritePermission();
            if (!flag)
            {
                return;
            }

            ImportFilePath = string.Empty;
            ImportFilePath = await PlatformService.PickZipFileAsync();
            if (string.IsNullOrEmpty(ImportFilePath))
            {
                return;
            }
            ShowImport = true;
        }

        private async Task ConfirmImport()
        {
            ShowImport = false;
            if (string.IsNullOrEmpty(ImportFilePath))
            {
                await AlertService.Error(I18n.T("Export.Import.Fail"));
                return;
            }

            try
            {
                List<DiaryModel>? diaries = await AppDataService.ImportJsonFileAsync(ImportFilePath);
                if (diaries == null || !diaries.Any())
                {
                    await AlertService.Error(I18n.T("Export.Import.Fail"));
                    return;
                }

                await DiaryService.ImportAsync(diaries);
                await AlertService.Success(I18n.T("Export.Import.Success"));
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e.StackTrace}");
                await AlertService.Error(I18n.T("Export.Import.Fail"));
            }
        }
    }
}
