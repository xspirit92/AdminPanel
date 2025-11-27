import { CommonModule, NgClass } from '@angular/common';
import { 
  Component, Input, Output, EventEmitter, ElementRef, ViewChild, 
  AfterViewInit, OnDestroy, OnChanges, SimpleChanges, OnInit 
} from '@angular/core';
import flatpickr from 'flatpickr';
import { Russian } from 'flatpickr/dist/l10n/ru.js';
import "flatpickr/dist/flatpickr.css";
import { LabelComponent } from '../form/label/label.component';

// Интерфейсы для типизации
export interface DateRange {
  startDate: string;
  endDate: string;
  selectedDates: Date[];
  displayValue: string;
}

export interface DateRangePreset {
  key: string;
  label: string;
  displayName: string;
  getRange: () => { startDate: Date; endDate: Date };
}

export type FlatpickrInstance = {
  destroy: () => void;
  close: () => void;
  open: () => void;
  setDate: (dates: Date[] | string, triggerChange?: boolean) => void;
  clear: () => void;
};

@Component({
  selector: 'app-date-range-picker',
  imports: [CommonModule, LabelComponent, NgClass],
  templateUrl: './date-range-picker.component.html',
  styles: ``
})
export class DateRangePickerComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {
  // Inputs
  @Input() id!: string;
  @Input() defaultStartDate?: string | Date;
  @Input() defaultEndDate?: string | Date;
  @Input() label?: string;
  @Input() placeholder: string = 'Выберите период';
  @Input() dateFormat: string = 'd.m.Y';
  @Input() resetTrigger?: boolean;

  // Outputs
  @Output() dateRangeChange = new EventEmitter<DateRange>();

  // View Children
  @ViewChild('dateInput', { static: false }) dateInput!: ElementRef<HTMLInputElement>;
  @ViewChild('dropdownContainer', { static: false }) dropdownContainer!: ElementRef<HTMLDivElement>;

  // Состояние компонента
  showDropdown = false;
  selectedPreset: string | null = null;
  currentDisplayValue: string = '';

  // Приватные свойства
  private flatpickrInstance: FlatpickrInstance | null = null;
  private isInitialized = false;

  // Предустановленные периоды
  readonly dateRangePresets: DateRangePreset[] = this.createDateRangePresets();

  ngOnInit(): void {
    this.isInitialized = true;
  }

  ngAfterViewInit(): void {
    this.initializeEventListeners();
    this.initializeFromDefaults();
  }

  ngOnDestroy(): void {
    this.cleanupEventListeners();
    this.destroyFlatpickr();
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.handleResetTrigger(changes);
    this.handleDefaultDateChanges(changes);
  }

  /**
   * Публичные методы
   */
  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  selectPreset(preset: DateRangePreset): void {
    if (preset.key === 'custom') {
      this.openCustomDatePicker();
    } else {
      this.applyPresetSelection(preset);
    }
  }

  clearSelection(): void {
    this.resetSelection();
  }

  /**
   * Приватные методы - инициализация и очистка
   */
  private initializeEventListeners(): void {
    document.addEventListener('click', this.handleClickOutside.bind(this));
  }

  private cleanupEventListeners(): void {
    document.removeEventListener('click', this.handleClickOutside.bind(this));
  }

  private handleClickOutside(event: Event): void {
    if (this.dropdownContainer && !this.dropdownContainer.nativeElement.contains(event.target as Node)) {
      this.showDropdown = false;
    }
  }

  /**
   * Обработчики изменений
   */
  private handleResetTrigger(changes: SimpleChanges): void {
    if (changes['resetTrigger']?.currentValue === true) {
      this.resetSelection();
    }
  }

  private handleDefaultDateChanges(changes: SimpleChanges): void {
    const defaultStartChanged = changes['defaultStartDate'];
    const defaultEndChanged = changes['defaultEndDate'];

    if ((defaultStartChanged || defaultEndChanged) && !this.isFirstChange(changes)) {
      if (!this.defaultStartDate && !this.defaultEndDate) {
        this.resetSelection();
      } else if (this.defaultStartDate && this.defaultEndDate) {
        this.initializeFromDefaults();
      }
    }
  }

  private isFirstChange(changes: SimpleChanges): boolean {
    return Object.values(changes).some(change => change?.firstChange);
  }

  /**
   * Сброс и инициализация
   */
  private resetSelection(): void {
    this.selectedPreset = null;
    this.currentDisplayValue = '';
    this.showDropdown = false;
    this.destroyFlatpickr();
    this.updateInputDisplay();

    this.emitDateRangeChange({
      startDate: '',
      endDate: '',
      selectedDates: [],
      displayValue: ''
    });
  }

  private initializeFromDefaults(): void {
    if (this.defaultStartDate && this.defaultEndDate) {
      try {
        const start = this.parseDate(this.defaultStartDate);
        const end = this.parseDate(this.defaultEndDate);

        const matchedPreset = this.findMatchingPreset(start, end);

        if (matchedPreset) {
          this.selectedPreset = matchedPreset.key;
          this.currentDisplayValue = matchedPreset.displayName;
        } else {
          this.selectedPreset = 'custom';
          this.currentDisplayValue = this.formatDateRange(start, end);
        }

        this.updateInputDisplay();
      } catch (error) {
        console.error('Error initializing from defaults:', error);
        this.resetSelection();
      }
    }
  }

  /**
   * Работа с Flatpickr
   */
  private openCustomDatePicker(): void {
    this.showDropdown = false;
    this.selectedPreset = 'custom';
    this.currentDisplayValue = 'Выберите даты...';
    this.updateInputDisplay();
    this.initializeFlatpickr();
  }

  private initializeFlatpickr(): void {
    this.destroyFlatpickr();

    const inputElement = this.dateInput.nativeElement;

    try {
      this.flatpickrInstance = flatpickr(inputElement, {
        mode: 'range',
        dateFormat: this.dateFormat,
        allowInput: false,
        clickOpens: true,
        defaultDate: this.getDefaultDates(),
        locale: Russian,
        static: false,
        inline: false,
        onChange: this.handleFlatpickrChange.bind(this),
        onClose: this.handleFlatpickrClose.bind(this)
      }) as unknown as FlatpickrInstance;

      // Открываем календарь
      setTimeout(() => {
        if (this.flatpickrInstance) {
          this.flatpickrInstance.open();
        }
      }, 0);
    } catch (error) {
      console.error('Error initializing Flatpickr:', error);
    }
  }

  private handleFlatpickrChange(selectedDates: Date[], dateStr: string): void {
    if (selectedDates.length === 2) {
      const [startDate, endDate] = selectedDates;
      
      const utcStartDate = this.toUTCDate(startDate);
      const utcEndDate = this.toUTCDate(endDate);

      this.selectedPreset = 'custom';
      this.currentDisplayValue = dateStr;
      this.updateInputDisplay();

      this.emitDateRangeChange({
        startDate: this.formatToISODate(utcStartDate),
        endDate: this.formatToISODate(utcEndDate),
        selectedDates: [utcStartDate, utcEndDate],
        displayValue: dateStr
      });

      // Закрываем flatpickr после выбора
      setTimeout(() => {
        if (this.flatpickrInstance) {
          this.flatpickrInstance.close();
        }
      }, 100);
    }
  }

  private handleFlatpickrClose(selectedDates: Date[]): void {
    if (selectedDates.length !== 2) {
      this.resetSelection();
    }
    
    setTimeout(() => {
      this.destroyFlatpickr();
    }, 100);
  }

  private destroyFlatpickr(): void {
    if (this.flatpickrInstance) {
      try {
        this.flatpickrInstance.destroy();
      } catch (error) {
        console.warn('Error destroying Flatpickr instance:', error);
      } finally {
        this.flatpickrInstance = null;
      }
    }
  }

  /**
   * Работа с пресетами
   */
  private createDateRangePresets(): DateRangePreset[] {
    return [
      this.createPreset('today', 'Сегодня', 'Сегодня', 0, 'day'),
      this.createPreset('yesterday', 'Вчера', 'Вчера', -1, 'day'),
      this.createPreset('currentWeek', 'Текущая неделя', 'Текущая неделя', 0, 'week'),
      this.createPreset('lastWeek', 'Прошлая неделя', 'Прошлая неделя', -1, 'week'),
      this.createPreset('currentMonth', 'Текущий месяц', 'Текущий месяц', 0, 'month'),
      this.createPreset('lastMonth', 'Прошлый месяц', 'Прошлый месяц', -1, 'month'),
      this.createPreset('currentYear', 'Текущий год', 'Текущий год', 0, 'year'),
      this.createPreset('lastYear', 'Прошлый год', 'Прошлый год', -1, 'year'),
      {
        key: 'custom',
        label: 'Произвольный диапазон',
        displayName: 'Произвольный диапазон',
        getRange: () => {
          const today = new Date();
          return { startDate: today, endDate: today };
        }
      }
    ];
  }

  private createPreset(key: string, label: string, displayName: string, offset: number, unit: 'day' | 'week' | 'month' | 'year'): DateRangePreset {
    return {
      key,
      label,
      displayName,
      getRange: () => this.calculateDateRange(offset, unit)
    };
  }

  private calculateDateRange(offset: number, unit: 'day' | 'week' | 'month' | 'year'): { startDate: Date; endDate: Date } {
    const now = new Date();
    
    switch (unit) {
      case 'day':
        return this.getDayRange(now, offset);
      case 'week':
        return this.getWeekRange(now, offset);
      case 'month':
        return this.getMonthRange(now, offset);
      case 'year':
        return this.getYearRange(now, offset);
      default:
        return { startDate: now, endDate: now };
    }
  }

  private getDayRange(date: Date, offset: number): { startDate: Date; endDate: Date } {
    const targetDate = new Date(date);
    targetDate.setDate(targetDate.getDate() + offset);
    return { startDate: targetDate, endDate: targetDate };
  }

  private getWeekRange(date: Date, offset: number): { startDate: Date; endDate: Date } {
    const targetDate = new Date(date);
    targetDate.setDate(targetDate.getDate() + offset * 7);
    
    const startOfWeek = new Date(targetDate);
    const day = startOfWeek.getDay();
    const diff = startOfWeek.getDate() - day + (day === 0 ? -6 : 1);
    startOfWeek.setDate(diff);
    
    const endOfWeek = new Date(startOfWeek);
    endOfWeek.setDate(startOfWeek.getDate() + 6);
    
    return { startDate: startOfWeek, endDate: endOfWeek };
  }

  private getMonthRange(date: Date, offset: number): { startDate: Date; endDate: Date } {
    const targetDate = new Date(date);
    targetDate.setMonth(targetDate.getMonth() + offset);
    
    const startOfMonth = new Date(targetDate.getFullYear(), targetDate.getMonth(), 1);
    const endOfMonth = new Date(targetDate.getFullYear(), targetDate.getMonth() + 1, 0);
    
    return { startDate: startOfMonth, endDate: endOfMonth };
  }

  private getYearRange(date: Date, offset: number): { startDate: Date; endDate: Date } {
    const year = date.getFullYear() + offset;
    const startOfYear = new Date(year, 0, 1);
    const endOfYear = new Date(year, 11, 31);
    
    return { startDate: startOfYear, endDate: endOfYear };
  }

  private applyPresetSelection(preset: DateRangePreset): void {
    const range = preset.getRange();
    const utcStartDate = this.toUTCDate(range.startDate);
    const utcEndDate = this.toUTCDate(range.endDate);

    this.selectedPreset = preset.key;
    this.showDropdown = false;
    this.currentDisplayValue = preset.displayName;
    this.updateInputDisplay();

    this.emitDateRangeChange({
      startDate: this.formatToISODate(utcStartDate),
      endDate: this.formatToISODate(utcEndDate),
      selectedDates: [utcStartDate, utcEndDate],
      displayValue: preset.displayName
    });
  }

  private findMatchingPreset(startDate: Date, endDate: Date): DateRangePreset | null {
    for (const preset of this.dateRangePresets) {
      if (preset.key === 'custom') continue;

      const presetRange = preset.getRange();
      const presetStart = this.toUTCDate(presetRange.startDate);
      const presetEnd = this.toUTCDate(presetRange.endDate);

      if (this.areDatesEqual(presetStart, startDate) && this.areDatesEqual(presetEnd, endDate)) {
        return preset;
      }
    }
    return null;
  }

  /**
   * Утилиты для работы с датами
   */
  private getDefaultDates(): Date[] | undefined {
    if (this.defaultStartDate && this.defaultEndDate) {
      try {
        const start = this.parseDate(this.defaultStartDate);
        const end = this.parseDate(this.defaultEndDate);
        return [start, end];
      } catch (error) {
        console.warn('Invalid default dates provided');
        return undefined;
      }
    }
    return undefined;
  }

  private parseDate(date: string | Date): Date {
    if (typeof date === 'string') {
      if (date.match(/^\d{4}-\d{2}-\d{2}$/)) {
        const [year, month, day] = date.split('-').map(Number);
        return new Date(Date.UTC(year, month - 1, day, 12, 0, 0, 0));
      }
      const parsed = new Date(date);
      if (isNaN(parsed.getTime())) {
        throw new Error(`Invalid date string: ${date}`);
      }
      return parsed;
    }
    return date;
  }

  private toUTCDate(date: Date): Date {
    return new Date(Date.UTC(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      12, 0, 0, 0
    ));
  }

  private formatToISODate(date: Date): string {
    const year = date.getUTCFullYear();
    const month = String(date.getUTCMonth() + 1).padStart(2, '0');
    const day = String(date.getUTCDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private formatDateRange(startDate: Date, endDate: Date): string {
    try {
      const displayFormat = new Intl.DateTimeFormat('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
      const startStr = displayFormat.format(startDate);
      const endStr = displayFormat.format(endDate);
      return `${startStr} - ${endStr}`;
    } catch (error) {
      console.error('Error formatting date range:', error);
      return 'Неверный формат даты';
    }
  }

  private areDatesEqual(date1: Date, date2: Date): boolean {
    return date1.getUTCFullYear() === date2.getUTCFullYear() &&
           date1.getUTCMonth() === date2.getUTCMonth() &&
           date1.getUTCDate() === date2.getUTCDate();
  }

  /**
   * Вспомогательные методы
   */
  private updateInputDisplay(): void {
    if (this.dateInput?.nativeElement) {
      this.dateInput.nativeElement.value = this.currentDisplayValue;
    }
  }

  private emitDateRangeChange(range: DateRange): void {
    this.dateRangeChange.emit(range);
  }
}