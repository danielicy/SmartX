import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { TweetModule } from '../tweet/tweet.module';
import { LoginComponent, RegisterComponent } from '../usermanagment/components';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class HomeRoutingModule { }
