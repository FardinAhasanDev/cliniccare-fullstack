import { Routes } from '@angular/router';
import { DoctorsComponets } from './components/doctors.componets/doctors.componets';

export const routes: Routes = [
  {path:'',redirectTo:'doctors',pathMatch:'full'},
  {path:'doctors',component:DoctorsComponets},
  {path:'**',redirectTo:'doctors'}
];
