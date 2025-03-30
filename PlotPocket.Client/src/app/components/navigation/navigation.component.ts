import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [RouterLinkActive, RouterLink, CommonModule],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.html',
})
export class NavigationComponent {
  constructor(public authService: AuthService) {}
}
