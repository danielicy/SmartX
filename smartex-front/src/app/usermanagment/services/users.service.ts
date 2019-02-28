import { environment } from '../../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }
  appiUrl = environment.appUrl;

  getAll() {

    return this.http.get<User[]>(this.appiUrl + `/users`);
  }

  getById(id: number) {
    return this.http.get(this.appiUrl + `/users/${id}`);
  }

  register(user: User) {
    return this.http.post(this.appiUrl + `/users/register`, user);
  }

  update(user: User) {
    return this.http.put(this.appiUrl + `/users/${user.id}`, user);
  }

  delete(id: number) {
    return this.http.delete(this.appiUrl + `/users/${id}`);
  }
}
