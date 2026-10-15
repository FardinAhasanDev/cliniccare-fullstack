import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { Doctor } from '../models/doctor';
import { Department } from '../models/department';
import { DoctorDTO } from '../models/doctor-dto';

@Service()
export class DoctorService {
  private apiUrl = 'http://localhost:5272/api/Doctors';
  private http = inject(HttpClient);

  getDoctors(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.apiUrl);
  }

  getDoctorById(id: number): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.apiUrl}/${id}`);
  }

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/departments`);
  }

  saveDoctor(
    doctorData: any,
    imageFile: File | null,
    specializations: DoctorDTO[],
  ): Observable<Doctor> {
    const formdata = this.buildFormData(doctorData, imageFile, specializations);
    return this.http.post<Doctor>(this.apiUrl, formdata);
  }

  updateDoctor(
    id: number,
    doctorData: any,
    imageFile: File | null,
    specializations: DoctorDTO[],
  ): Observable<void> {
    const formdata = this.buildFormData(doctorData, imageFile, specializations);
    formdata.append('DoctorId', id.toString());
    return this.http.put<void>(`${this.apiUrl}/${id}`, formdata);
  }

  deleteDoctor(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private buildFormData(
    data: any,
    imageFile: File | null,
    specializations: DoctorDTO[],
  ): FormData {
    const formdata = new FormData();
    formdata.append('DoctorName', data.doctorName);
    formdata.append('IsActive', data.isActive ? 'true' : 'false');
    formdata.append('SpecializationsJson', JSON.stringify(specializations));

    const rawDate = data.joinDate ?? data.JoinDate;
    let joinDateFormatted = '';
    if (rawDate) {
      const parsedDate = new Date(rawDate);
      if (!isNaN(parsedDate.getTime())) {
        joinDateFormatted = parsedDate.toISOString().split('T')[0];
      }
    }
    formdata.append('JoinDate', joinDateFormatted);

    if (imageFile) {
      formdata.append('ImageFile', imageFile);
    } else if (data.imageUrl) {
      formdata.append('ImageUrl', data.imageUrl);
    }
    formdata.append('DoctorSpecializationsString', JSON.stringify(specializations));
    return formdata;
    
  }
}