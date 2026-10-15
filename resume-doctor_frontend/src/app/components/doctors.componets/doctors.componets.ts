import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, computed, inject, OnInit, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DoctorService } from '../../service/doctor.service';
import { Doctor } from '../../models/doctor';
import { Department } from '../../models/department';
import { DoctorDTO } from '../../models/doctor-dto';

@Component({
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  selector: 'app-doctors.componets',
  styleUrl: './doctors.componets.css',
  templateUrl: './doctors.componets.html',
})
export class DoctorsComponets implements OnInit {
  private fb = inject(FormBuilder);
  private doctorService = inject(DoctorService);
  private cdr = inject(ChangeDetectorRef);

  doctors = signal<Doctor[]>([]);
  departments = signal<Department[]>([]);
  selectedFile = signal<File | null>(null);
  imagePreview = signal<string | null>(null);
  isEditMode = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  editDoctorId = signal<number | null>(null);
  doctorForm!: FormGroup;
  serverUrl = 'http://localhost:5272';

  drawerOpen = signal<boolean>(false);
  search = signal<string>('');
  statusFilter = signal<'all' | 'active' | 'inactive'>('all');

  filteredDoctors = computed(() => {
    const q = this.search().trim().toLowerCase();
    const f = this.statusFilter();

    return this.doctors().filter((d) => {
      const statusOk = f === 'all' || (f === 'active') === d.isActive;
      const textOk =
        !q ||
        d.doctorName?.toLowerCase().includes(q) ||
        d.doctorSpecializations?.some((s) =>
          s.department?.departmentName?.toLowerCase().includes(q),
        );
      return statusOk && textOk;
    });
  });

  activeCount = computed(() => this.doctors().filter((d) => d.isActive).length);
  inactiveCount = computed(() => this.doctors().length - this.activeCount());

  ngOnInit(): void {
    this.inForm();
    this.loadDoctors();
    this.loadDepartments();
    this.cdr.detectChanges();
  }

  openAdd(): void {
    this.resetForm();
    this.drawerOpen.set(true);
  }

  openEdit(doctor: Doctor): void {
    this.editDoctor(doctor);
    this.drawerOpen.set(true);
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.resetForm();
  }

  loadDepartments(): void {
    this.doctorService.getDepartments().subscribe({
      next: (data) => this.departments.set(data),
      error: (err) => console.error(err),
    });
  }

  get doctorSpecialization(): FormArray {
    return this.doctorForm.get('doctorSpecialization') as FormArray;
  }

  loadDoctors(): void {
    this.doctorService.getDoctors().subscribe({
      next: (data) => {
        this.doctors.set(data);
      },
      error: (err) => console.error(err),
    });
  }

  inForm() {
    this.doctorForm = this.fb.group({
      doctorName: ['', [Validators.required]],
      isActive: [true],
      joinDate: ['', [Validators.required]],
      doctorSpecialization: this.fb.array([]),
    });
  }

  deleteDoctor(id: number): void {
    if (confirm('Are you sure to delete this doctor')) {
      this.doctorService.deleteDoctor(id).subscribe({
        next: () => {
          this.loadDoctors();
        },
        error: (err) => {
          console.error(err);
        },
      });
    }
  }

  editDoctor(doctor: Doctor): void {
    this.isEditMode.set(true);
    this.editDoctorId.set(doctor.doctorId);
    this.selectedFile.set(null);
    this.imagePreview.set(doctor.imageUrl ? `${this.serverUrl}${doctor.imageUrl}` : null);
    let joinDateFormatted = '';
    if (doctor.joinDate) {
      const d = new Date(doctor.joinDate);
      if (!isNaN(d.getTime())) {
        joinDateFormatted = d.toISOString().split('T')[0];
      }
    }
    this.doctorForm.patchValue({
      doctorName: doctor.doctorName,
      isActive: doctor.isActive,
      joinDate: joinDateFormatted,
    });
    this.doctorSpecialization.clear();
    if (doctor.doctorSpecializations?.length) {
      doctor.doctorSpecializations.forEach((spec) => {
        this.addSpecialization(spec.departmentId, spec.yearsOfExperience);
      });
    }
  }

  onSubmit(): void {
    if (this.doctorForm.invalid) {
      this.doctorForm.markAllAsTouched();
      return;
    }
    const formValue = this.doctorForm.value;
    const specializationData: DoctorDTO[] = formValue.doctorSpecialization;
    const editId = this.editDoctorId();
    if (this.isEditMode() && editId !== null) {
      this.doctorService
        .updateDoctor(editId, formValue, this.selectedFile(), specializationData)
        .subscribe({
          next: () => {
            this.loadDoctors();
            this.closeDrawer();
          },
          error: (err) => {
            console.error('Error in Updating doctor: ' + err);
          },
        });
    } else {
      this.doctorService.saveDoctor(formValue, this.selectedFile(), specializationData).subscribe({
        next: () => {
          this.loadDoctors();
          this.closeDrawer();
        },
        error: (err) => {
          console.error('Error in saving doctor: ' + err);
        },
      });
    }
  }

  resetForm() {
    this.isEditMode.set(false);
    this.editDoctorId.set(null);
    this.selectedFile.set(null);
    this.imagePreview.set(null);
    this.doctorForm.reset({ isActive: true });
    this.doctorSpecialization.clear();
  }

  removeSpecialization(index: number): void {
    this.doctorSpecialization.removeAt(index);
  }

  newSpecialization(departmentId: number = 0, yearsOfExperience: number = 0): FormGroup {
    return this.fb.group({
      departmentId: [departmentId, [Validators.required, Validators.min(1)]],
      yearsOfExperience: [yearsOfExperience, [Validators.required, Validators.min(1)]],
    });
  }

  addSpecialization(departmentId: number = 0, yearsOfExperience: number = 0): void {
    this.doctorSpecialization.push(this.newSpecialization(departmentId, yearsOfExperience));
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.selectedFile.set(file);
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }
}
