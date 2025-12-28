import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; 
import { LoginService } from '../../services/login';

// PrimeNG 20 Imports
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, AvatarModule, ButtonModule, TooltipModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent implements OnInit {
  
  managerName: string = ''; 
  userInitial: string = 'U'; // אות ראשונה לאווטאר

  constructor(private router: Router, private loginService: LoginService) { } 

  ngOnInit() {
    const username = this.loginService.getCurrentUsername();
    
    if (username) {
        this.managerName = username;
        this.userInitial = username.charAt(0).toUpperCase();
    } else {
        this.managerName = 'מנהל אורח';
    }
  }

  logout(): void {
    this.loginService.logout();
  }
}