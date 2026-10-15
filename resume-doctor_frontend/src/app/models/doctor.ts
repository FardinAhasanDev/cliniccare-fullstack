import { DoctorSpecialization } from "./doctor-specialization";

export interface Doctor {
  doctorId:number;
  doctorName:string;
  isActive:boolean;
  joinDate:string;
  imageUrl?:string;
  imageName?:string;
  doctorSpecializations:DoctorSpecialization[];
}
