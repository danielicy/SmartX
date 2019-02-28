
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { first } from 'rxjs/operators';

import { User } from '../usermanagment/models';
import { UsersService, AuthenticationService } from '../usermanagment/services';
import { forEach } from '@angular/router/src/utils/collection';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  currentUser: User;
  currentUserSubscription: Subscription;
  users: User[] = [];

  constructor(private authenticationService: AuthenticationService,
    private userService: UsersService) {
    this.currentUserSubscription = this.authenticationService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  ngOnInit() {
    this.loadAllUsers();
  }

  ngOnDestroy() {
    // unsubscribe to ensure no memory leaks
    this.currentUserSubscription.unsubscribe();
  }

  followUser(id: number) {
   /* this.userService.followUser(this.currentUser, id).pipe(first()).subscribe(() => {
      this.loadAllUsers()
    });*/
  }


  //https://stackoverflow.com/questions/28407392/automapper-many-to-many-mapping
  private loadAllUsers() {
    this.userService.getAll().pipe(first()).subscribe(users => {

      this.users = users.filter(
        user => user.username != this.currentUser.username);

      this.users.forEach(user => this.isUserFollowed(user));
    });
  }

  isUserFollowed(user: User) {
    this.currentUser.contacts.forEach(contact => {
     /* if (contact.followedId === user.id) {
        user.isfollowed = true;
      }*/
    });
  }


}
