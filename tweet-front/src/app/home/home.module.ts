import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { CommonModule } from '@angular/common';
 


import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';

import { AlertComponent } from '../usermanagment/components/alert/alert.component';
import { JwtInterceptor } from '../usermanagment/helpers/jwt';
import { ErrorInterceptor } from '../usermanagment/helpers/error';


import { LoginComponent } from '../usermanagment/components/login/login.component';
import { RegisterComponent } from '../usermanagment/components/register/register.component';

import { TweetModule } from '../tweet/tweet.module';


@NgModule({
  declarations: [
    HomeComponent,
    RegisterComponent,
    LoginComponent    
    
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    HttpClientModule,
    CommonModule,
    HomeRoutingModule,
    TweetModule
    
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },

    // provider used to create fake backend
    //fakeBackendProvider
  ]
})
export class HomeModule { }
