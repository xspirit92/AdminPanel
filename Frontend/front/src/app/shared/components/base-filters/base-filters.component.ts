import { 
  Component, 
  Input, 
  Output, 
  EventEmitter, 
  SimpleChanges,
  ChangeDetectionStrategy 
} from '@angular/core';
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

export type FilterType = 'select' | 'date-range' | 'text';

export interface FilterField {
  key: string;
  label: string;
  type: FilterType;
  options?: FilterOption[];
  placeholder?: string;
}

export interface DateRangeValue {
  startDate?: string;
  endDate?: string;
}

export type FilterValue = string | number | DateRangeValue | null | undefined;

export interface FilterValues {
  [key: string]: FilterValue;
}

@Component({
  selector: 'app-base-filters',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    ModalComponent,
    LabelComponent,
    SelectComponent,
    DateRangePickerComponent
  ],
  templateUrl: './base-filters.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BaseFiltersComponent {
  // Inputs
  @Input() filters: FilterField[] = [];
  @Input() filterValues: FilterValues = {};
  @Input() hasActiveFilters = false;
  @Input() isFilterModalOpen = false;
  @Input() resetTrigger = false;

  // Outputs
  @Output() filtersChange = new EventEmitter<FilterValues>();
  @Output() filtersApply = new EventEmitter<void>();
  @Output() filtersReset = new EventEmitter<void>();
  @Output() modalToggle = new EventEmitter<boolean>();

  // Делаем публичным для использования в шаблоне
  tempFilterValues: FilterValues = {};

  ngOnChanges(changes: SimpleChanges): void {
    // Инициализируем временные значения при открытии модалки
    if (changes['isFilterModalOpen']?.currentValue === true) {
      this.tempFilterValues = { ...this.filterValues };
    }

    // Обрабатываем сброс фильтров
    if (changes['resetTrigger']?.currentValue === true) {
      this.tempFilterValues = {};
    }
  }

  /**
   * Обработчики событий модального окна
   */
  onOpenModal(): void {
    this.tempFilterValues = { ...this.filterValues };
    this.modalToggle.emit(true);
  }

  onCloseModal(): void {
    this.modalToggle.emit(false);
  }

  /**
   * Обработчики изменений фильтров в модалке
   */
  onFilterChange(key: string, value: FilterValue): void {
    this.tempFilterValues[key] = value;
  }

  onDateRangeChange(key: string, event: { startDate: string, endDate: string }): void {
    this.tempFilterValues[key] = {
      startDate: event.startDate,
      endDate: event.endDate
    };
  }

  /**
   * Действия с фильтрами
   */
  onApply(): void {
    // Эмитим новые значения фильтров
    this.filtersChange.emit({ ...this.tempFilterValues });
    // Эмитим событие применения
    this.filtersApply.emit();
    // Закрываем модалку
    this.modalToggle.emit(false);
  }

  onReset(): void {
    // Сбрасываем временные значения
    this.tempFilterValues = {};
    // Эмитим событие сброса
    this.filtersReset.emit();
    // Закрываем модалку
    this.modalToggle.emit(false);
  }

  /**
   * Вспомогательные методы для шаблона
   */
  hasFilterValue(key: string): boolean {
    const value = this.tempFilterValues[key];
    
    if (value === null || value === undefined || value === '') {
      return false;
    }

    // Проверяем DateRangeValue
    if (typeof value === 'object' && 'startDate' in value) {
      return !!(value.startDate || value.endDate);
    }

    return true;
  }

  /**
   * Методы для типизации (удобно для шаблона)
   */
  isDateRangeValue(value: FilterValue): value is DateRangeValue {
    return typeof value === 'object' && value !== null && ('startDate' in value || 'endDate' in value);
  }

  getDateRangeValue(key: string): DateRangeValue {
    const value = this.tempFilterValues[key];
    return this.isDateRangeValue(value) ? value : {};
  }
}