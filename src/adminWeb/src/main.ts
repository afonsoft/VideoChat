import { withHttpErrorConfig } from '@abp/ng.theme.shared';
import { withValidationBluePrint, provideAbpThemeShared } from '@abp/ng.theme.shared';
import { provideZoneChangeDetection } from "@angular/core";
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { provideThemeLeptonX } from '@abp/ng.theme.lepton-x';
import { provideSideMenuLayout } from '@abp/ng.theme.lepton-x/layouts';

bootstrapApplication(AppComponent, {
  ...appConfig,
  providers: [
    provideZoneChangeDetection(),
    ...appConfig.providers,
    provideAbpThemeShared(
      withValidationBluePrint({
        wrongPassword: 'Please choose 1q2w3E*'
      })
    ),
    provideThemeLeptonX(),
    provideSideMenuLayout()
  ]
}).catch(err => console.error(err));
