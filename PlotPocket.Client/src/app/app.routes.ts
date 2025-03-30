import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { RegistrationComponent } from './components/register/register.component';
import { LoginComponent } from './components/login/login.component';
import { MoviesComponent } from './components/movies/movies.component';
import { TrendingComponent } from './components/trending/trending.component';
import { TvShowsComponent } from './components/tv-shows/tv-shows.component';
import { LogoutComponent } from './components/logout/logout.component';
import { BookmarkComponent } from './components/bookmark/bookmark.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'register', component: RegistrationComponent },
  { path: 'login', component: LoginComponent },
  { path: 'movies', component: MoviesComponent },
  { path: 'trending', component: TrendingComponent },
  { path: 'tv-shows', component: TvShowsComponent },
  { path: 'login', component: LoginComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'bookmark', component: BookmarkComponent },
];
