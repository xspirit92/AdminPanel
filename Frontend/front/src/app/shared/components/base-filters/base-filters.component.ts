import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalComponent } from '../ui/modal/modal.component';
import { ButtonComponent } from '../ui/button/button.component';
import { LabelComponent } from '../form/label/label.component';
import { SelectComponent } from '../form/select/select.component';
import { DateRangePickerComponent } from '../date-range-picker/date-range-picker.component';

export interface FilterOption {
  value: any;
  label: string;
}

export interface FilterField {
  key: string;
  label: string;
  type: 'select' | 'date-range' | 'text';
  options?: FilterOption[];
  placeholder?: string;
}

@Component({
  selector: 'app-base-filters',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    ModalComponent,
    ButtonComponent,
    LabelComponent,
    SelectComponent,
    DateRangePickerComponent
  ],
  templateUrl: './base-filters.component.html'
})
export class BaseFiltersComponent {
  @Input() filters: FilterField[] = [];
  @Input() filterValues: { [key: string]: any } = {};
  @Input() hasActiveFilters = false;
  @Input() isFilterModalOpen = false;
  
  @Output() filterChange = new EventEmitter<{ [key: string]: any }>();
  @Output() applyFilters = new EventEmitter<void>();
  @Output() clearFilters = new EventEmitter<void>();
  @Output() openFilterModal = new EventEmitter<void>();
  @Output() closeFilterModal = new EventEmitter<void>();

  onFilterChange(key: string, value: any): void {
    this.filterValues[key] = value;
    this.filterChange.emit(this.filterValues);
  }

  onApply(): void {
    this.applyFilters.emit();
  }

  onClear(): void {
    this.clearFilters.emit();
  }

  onOpenModal(): void {
    this.openFilterModal.emit();
  }

  onCloseModal(): void {
    this.closeFilterModal.emit();
  }

  onDateRangeChange(key: string, event: { startDate: string, endDate: string, selectedDates: Date[], displayValue: string }): void {
    this.filterValues[key] = {
      startDate: event.startDate,
      endDate: event.endDate
    };
    this.filterChange.emit(this.filterValues);
  }
}