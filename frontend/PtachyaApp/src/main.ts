import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app';
import { appConfig } from './app/app.config';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura'; // Theme חדש

bootstrapApplication(AppComponent, {
  ...appConfig,
  providers: [
    ...(appConfig.providers || []),
    providePrimeNG({
      theme: { preset: Aura, options: { ripple: true } }
    })
  ]
})
.catch(err => console.error(err));
