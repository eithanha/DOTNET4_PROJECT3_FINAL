import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EmailLoginDetails } from '../../models/email-login-details';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegistrationComponent implements OnInit {
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  errorMessage: string = '';
  passwordVisible: boolean = false;
  confirmPasswordVisible: boolean = false;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {}

  togglePasswordVisibility() {
    this.passwordVisible = !this.passwordVisible;
  }

  toggleConfirmPasswordVisibility() {
    this.confirmPasswordVisible = !this.confirmPasswordVisible;
  }

  register() {
    if (!this.email || !this.password || !this.confirmPassword) {
      this.errorMessage = 'Please Fill In All Fields';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords Do Not Match';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'Password Must Be At Least 6 Characters Long';
      return;
    }

    const registerDetails: EmailLoginDetails = {
      email: this.email,
      password: this.password,
    };

    this.authService.register(registerDetails).subscribe({
      next: () => {
        Swal.fire({
          title: 'Registration Successful!',
          text: 'You Can Now Log In.',
          icon: 'success',
          confirmButtonText: 'OK',
        });
        this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Registration error:', error);
        let errorMessage = 'An Unknown Error Occurred. Please Try Again.';

        if (error.error && typeof error.error === 'string') {
          errorMessage = error.error;
        } else if (error.error && Array.isArray(error.error)) {
          errorMessage = error.error.join('\n');
        }

        Swal.fire({
          title: 'Registration Failed!',
          text: errorMessage,
          icon: 'error',
          confirmButtonText: 'OK',
        });
      },
    });
  }
}
