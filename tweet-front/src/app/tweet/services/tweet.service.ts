import { environment } from '../../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Tweet } from '../models';

@Injectable({
  providedIn: 'root'
})
export class TweetService {

  constructor(private http: HttpClient) { }
 appiUrl = environment.appUrl;
    getAll() {
      return this.http.get<Tweet[]>(this.appiUrl + `/tweets`);
    }

    //gets tweets user tweetd
    getById(id: number) {
        return this.http.get(this.appiUrl + `/tweets/${id}`);
    }

    //gets tweets by users I follow
	 getFollowedTweets(id: number) {
       return this.http.get(this.appiUrl + `/tweets/ifollow/${id}`);
    }	

    tweet(tweet: Tweet) {
        return this.http.post(this.appiUrl + `/tweets/tweet`, tweet);
    }

    update(tweet: Tweet) {
        return this.http.put(this.appiUrl + `/tweets/${tweet.id}`, tweet);
    }

    
}
