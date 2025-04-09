import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EmailLoginDetails } from '../../models/email-login-details';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  passwordVisible: boolean = false;

  constructor(private authService: AuthService, private router: Router) {}

  togglePasswordVisibility() {
    this.passwordVisible = !this.passwordVisible;
  }

  login() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please Enter Your Email And Password';
      return;
    }

    const loginDetails: EmailLoginDetails = {
      email: this.email,
      password: this.password,
    };

    this.authService.login(loginDetails).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (error: Error) => {
        this.errorMessage = 'Invalid Email Or Password';
        console.error('Login Error:', error);
      },
    });
  }
}
