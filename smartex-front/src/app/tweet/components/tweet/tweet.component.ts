import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { first } from 'rxjs/operators';


import { User } from '../../../usermanagment/models';
import { Tweet } from '../../models';
import { TweetService } from '../../services/tweet.service';

import { AlertService, AuthenticationService } from '../../../usermanagment/services';


@Component({
  selector: 'app-tweet',
  templateUrl: './tweet.component.html',
  styleUrls: ['./tweet.component.css']
})
export class TweetComponent implements OnInit, OnDestroy {
  msg: Tweet;

  tweets: any = [];

  currentUser: User;
  currentUserSubscription: Subscription;
  loading = false;
  submitted = false;

  constructor(
    private authenticationService: AuthenticationService,
    private tweeterService: TweetService,
    private alertService: AlertService
  ) {
    this.currentUserSubscription = this.authenticationService.currentUser.subscribe(user => {
      this.currentUser = user;
    });

  }

  ngOnInit() {
    this.loadAllTweets();
  }

  ngOnDestroy() {
    // unsubscribe to ensure no memory leaks

  }

  loadAllTweets(): void {
    this.tweeterService.getAll().pipe(first()).subscribe(tweets => {
      this.tweets = tweets;
    });
  }

  //loads tweets I follow
  loadTweetsIFollow(): void {
    this.tweeterService.getFollowedTweets(this.currentUser.id).pipe(first()).subscribe(tweets => {
      this.tweets = tweets;
    });
  }

  // loads tweets I Tweeted
  loadMyTweets(): void {
    this.tweeterService.getById(this.currentUser.id).pipe(first()).subscribe(tweets => {
      this.tweets = tweets;
    });
  }

  tweet(content: string): void {
    content = content.trim();
    if (!content) { return; }

    this.tweeterService.tweet({ content, userid: this.currentUser.id } as Tweet)
      .subscribe(data => {
        this.alertService.success('What a Tweet!!', true);
        this.tweets.push({ username: this.currentUser.username, content, userid: this.currentUser.id } as Tweet);
      },
        error => {
          this.alertService.error(error);
          this.loading = false;
        });


  }

}
