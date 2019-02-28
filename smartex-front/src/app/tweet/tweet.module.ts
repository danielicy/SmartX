import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TweetComponent } from '../tweet/components';

@NgModule({
  declarations: [TweetComponent],
  imports: [
    CommonModule
  ],
  exports: [
    TweetComponent
  ]
})
export class TweetModule { }
