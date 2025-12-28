import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app';

// PrimeNG Modules
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { BadgeModule } from 'primeng/badge';

// Angular Modules
import { HttpClientModule } from '@angular/common/http';

import { RouterModule } from '@angular/router';


@NgModule({
  imports: [
    BrowserModule,
   
    HttpClientModule,
  
    RouterModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    CardModule,
    MessageModule,
    BadgeModule,
    
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
