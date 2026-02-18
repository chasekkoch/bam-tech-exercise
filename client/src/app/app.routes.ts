import { Routes } from '@angular/router';
import { Home } from './home/home';
import { Personnel } from './personnel/personnel';
import { PersonDetail } from './person-detail/person-detail';
import { AddPerson } from './add-person/add-person';
import { AddDuty } from './add-duty/add-duty';
import { Admin } from './admin/admin';
import { LoginComponent } from './login/login';
import { RegisterComponent } from './register/register';
import { authGuard, adminGuard, guestGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: Home, title: 'Home - Stargate ACTS' },
  { path: 'login', component: LoginComponent, title: 'Login - Stargate ACTS', canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, title: 'Register - Stargate ACTS', canActivate: [guestGuard] },
  { path: 'personnel', component: Personnel, title: 'Personnel Directory - Stargate ACTS', canActivate: [authGuard] },
  { path: 'personnel/new', component: AddPerson, title: 'Add Person - Stargate ACTS', canActivate: [authGuard] },
  { path: 'personnel/:name', component: PersonDetail, title: 'Person Detail - Stargate ACTS', canActivate: [authGuard] },
  { path: 'duties/new', component: AddDuty, title: 'Add Duty - Stargate ACTS', canActivate: [authGuard] },
  { path: 'admin', component: Admin, title: 'Admin Dashboard - Stargate ACTS', canActivate: [adminGuard] },
  { path: '**', redirectTo: '' }
];



