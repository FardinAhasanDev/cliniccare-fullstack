import { Department } from "./department";

export interface DoctorSpecialization {
  doctorSpecializationId:number;
  doctorId:number;
  departmentId:number;
  yearsOfExperience:number;
  department:Department;
}
