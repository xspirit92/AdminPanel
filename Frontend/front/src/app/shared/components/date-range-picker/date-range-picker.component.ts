import { CommonModule, NgClass } from '@angular/common';
import { Component, Input, Output, EventEmitter, ElementRef, ViewChild, AfterViewInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import flatpickr from 'flatpickr';
import "flatpickr/dist/flatpickr.css";
import { LabelComponent } from '../form/label/label.component';

interface DateRangePreset {
  key: string;
  label: string;
  displayName: string;
  getRange: () => { startDate: Date; endDate: Date };
}

@Component({
  selector: 'app-date-range-picker',
  imports: [CommonModule, LabelComponent, NgClass],
  templateUrl: './date-range-picker.component.html',
  styles: ``
})
export class DateRangePickerComponent implements AfterViewInit, OnDestroy, OnChanges {

  @Input() id!: string;
  @Input() defaultStartDate?: string | Date;
  @Input() defaultEndDate?: string | Date;
  @Input() label?: string;
  @Input() placeholder?: string = 'Выберите период';
  @Input() dateFormat: string = 'd.m.Y';
  @Output() dateRangeChange = new EventEmitter<{ startDate: string, endDate: string, selectedDates: Date[], displayValue: string }>();

  @ViewChild('dateInput', { static: false }) dateInput!: ElementRef<HTMLInputElement>;
  @ViewChild('dropdownContainer', { static: false }) dropdownContainer!: ElementRef<HTMLDivElement>;

  showDropdown = false;
  selectedPreset: string | null = null;
  currentDisplayValue: string = '';
  private flatpickrInstance: any = null;

  // Предустановленные периоды
  dateRangePresets: DateRangePreset[] = [
    {
      key: 'today',
      label: 'Сегодня',
      displayName: 'Сегодня',
      getRange: () => {
        const today = new Date();
        return { startDate: today, endDate: today };
      }
    },
    {
      key: 'yesterday',
      label: 'Вчера',
      displayName: 'Вчера',
      getRange: () => {
        const yesterday = new Date();
        yesterday.setDate(yesterday.getDate() - 1);
        return { startDate: yesterday, endDate: yesterday };
      }
    },
    {
      key: 'currentWeek',
      label: 'Текущая неделя',
      displayName: 'Текущая неделя',
      getRange: () => {
        const today = new Date();
        const startOfWeek = new Date(today);
        startOfWeek.setDate(today.getDate() - today.getDay() + (today.getDay() === 0 ? -6 : 1));
        const endOfWeek = new Date(startOfWeek);
        endOfWeek.setDate(startOfWeek.getDate() + 6);
        return { startDate: startOfWeek, endDate: endOfWeek };
      }
    },
    {
      key: 'lastWeek',
      label: 'Прошлая неделя',
      displayName: 'Прошлая неделя',
      getRange: () => {
        const today = new Date();
        const startOfLastWeek = new Date(today);
        startOfLastWeek.setDate(today.getDate() - today.getDay() - 6);
        const endOfLastWeek = new Date(startOfLastWeek);
        endOfLastWeek.setDate(startOfLastWeek.getDate() + 6);
        return { startDate: startOfLastWeek, endDate: endOfLastWeek };
      }
    },
    {
      key: 'currentMonth',
      label: 'Текущий месяц',
      displayName: 'Текущий месяц',
      getRange: () => {
        const today = new Date();
        const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
        const endOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        return { startDate: startOfMonth, endDate: endOfMonth };
      }
    },
    {
      key: 'lastMonth',
      label: 'Прошлый месяц',
      displayName: 'Прошлый месяц',
      getRange: () => {
        const today = new Date();
        const startOfLastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        const endOfLastMonth = new Date(today.getFullYear(), today.getMonth(), 0);
        return { startDate: startOfLastMonth, endDate: endOfLastMonth };
      }
    },
    {
      key: 'currentYear',
      label: 'Текущий год',
      displayName: 'Текущий год',
      getRange: () => {
        const today = new Date();
        const startOfYear = new Date(today.getFullYear(), 0, 1);
        const endOfYear = new Date(today.getFullYear(), 11, 31);
        return { startDate: startOfYear, endDate: endOfYear };
      }
    },
    {
      key: 'lastYear',
      label: 'Прошлый год',
      displayName: 'Прошлый год',
      getRange: () => {
        const today = new Date();
        const startOfLastYear = new Date(today.getFullYear() - 1, 0, 1);
        const endOfLastYear = new Date(today.getFullYear() - 1, 11, 31);
        return { startDate: startOfLastYear, endDate: endOfLastYear };
      }
    },
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

  ngAfterViewInit() {
    document.addEventListener('click', this.handleClickOutside.bind(this));
    this.initializeFromDefaults();
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.handleClickOutside.bind(this));
    this.destroyFlatpickr();
  }

  ngOnChanges(changes: SimpleChanges) {
    if ((changes['defaultStartDate'] || changes['defaultEndDate']) && 
        this.defaultStartDate && this.defaultEndDate) {
      this.initializeFromDefaults();
    }
  }

  private initializeFromDefaults() {
    if (this.defaultStartDate && this.defaultEndDate) {
      const start = this.parseDate(this.defaultStartDate);
      const end = this.parseDate(this.defaultEndDate);
      
      // Ищем подходящий пресет по датам
      const matchedPreset = this.findMatchingPreset(start, end);
      
      if (matchedPreset) {
        this.selectedPreset = matchedPreset.key;
        this.currentDisplayValue = matchedPreset.displayName;
      } else {
        this.selectedPreset = 'custom';
        this.currentDisplayValue = this.formatDateRange(start, end);
      }
      
      this.updateInputDisplay();
    }
  }

  // Метод для поиска пресета по датам
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

  // Метод для сравнения дат (только дата, без времени)
  private areDatesEqual(date1: Date, date2: Date): boolean {
    return date1.getUTCFullYear() === date2.getUTCFullYear() &&
           date1.getUTCMonth() === date2.getUTCMonth() &&
           date1.getUTCDate() === date2.getUTCDate();
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  openCustomDatePicker() {
    this.showDropdown = false;
    this.selectedPreset = 'custom';
    this.currentDisplayValue = 'Выберите даты...';
    this.updateInputDisplay();

    // Используем существующий input для flatpickr
    const inputElement = this.dateInput.nativeElement;

    // Сначала уничтожаем предыдущий экземпляр flatpickr, если он существует
    this.destroyFlatpickr();

    this.flatpickrInstance = flatpickr(inputElement, {
      mode: 'range',
      static: false,
      inline: false,
      dateFormat: this.dateFormat,
      allowInput: false,
      clickOpens: true,
      defaultDate: this.getDefaultDates(),
      locale: {
        firstDayOfWeek: 1,
        weekdays: {
          shorthand: ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'],
          longhand: ['Воскресенье', 'Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота'],
        },
        months: {
          shorthand: ['Янв', 'Фев', 'Март', 'Апр', 'Май', 'Июнь', 'Июль', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'],
          longhand: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
        },
        rangeSeparator: ' до ',
      },
      onChange: (selectedDates, dateStr, instance) => {
        if (selectedDates.length === 2) {
          const startDate = selectedDates[0];
          const endDate = selectedDates[1];

          const utcStartDate = this.toUTCDate(startDate);
          const utcEndDate = this.toUTCDate(endDate);

          this.selectedPreset = 'custom';
          this.currentDisplayValue = dateStr;
          this.updateInputDisplay();

          this.dateRangeChange.emit({
            startDate: this.formatToISODate(utcStartDate),
            endDate: this.formatToISODate(utcEndDate),
            selectedDates: [utcStartDate, utcEndDate],
            displayValue: this.currentDisplayValue
          });

          // Закрываем flatpickr после выбора
          setTimeout(() => {
            instance.close();
          }, 100);
        }
      },
      onClose: (selectedDates, dateStr, instance) => {
        // Если даты не выбраны, сбрасываем значение
        if (selectedDates.length !== 2) {
          this.currentDisplayValue = '';
          this.selectedPreset = null;
          this.updateInputDisplay();
          
          this.dateRangeChange.emit({
            startDate: '',
            endDate: '',
            selectedDates: [],
            displayValue: ''
          });
        }
        
        // Уничтожаем flatpickr при закрытии
        setTimeout(() => {
          this.destroyFlatpickr();
        }, 100);
      }
    });

    // Открываем календарь
    setTimeout(() => {
      this.flatpickrInstance.open();
    }, 0);
  }

  private getDefaultDates(): Date[] | undefined {
    if (this.defaultStartDate && this.defaultEndDate) {
      const start = this.parseDate(this.defaultStartDate);
      const end = this.parseDate(this.defaultEndDate);
      return [start, end];
    }
    return undefined;
  }

  private destroyFlatpickr() {
    if (this.flatpickrInstance) {
      this.flatpickrInstance.destroy();
      this.flatpickrInstance = null;
    }
  }

  selectPreset(preset: DateRangePreset) {
    if (preset.key === 'custom') {
      this.openCustomDatePicker();
      return;
    }

    const range = preset.getRange();
    const utcStartDate = this.toUTCDate(range.startDate);
    const utcEndDate = this.toUTCDate(range.endDate);

    this.selectedPreset = preset.key;
    this.showDropdown = false;
    this.currentDisplayValue = preset.displayName; // Используем displayName вместо дат
    this.updateInputDisplay();

    this.dateRangeChange.emit({
      startDate: this.formatToISODate(utcStartDate),
      endDate: this.formatToISODate(utcEndDate),
      selectedDates: [utcStartDate, utcEndDate],
      displayValue: this.currentDisplayValue
    });
  }

  private updateInputDisplay() {
    if (this.dateInput && this.dateInput.nativeElement) {
      this.dateInput.nativeElement.value = this.currentDisplayValue;
    }
  }

  private handleClickOutside(event: Event) {
    if (this.dropdownContainer && !this.dropdownContainer.nativeElement.contains(event.target as Node)) {
      this.showDropdown = false;
    }
  }

  /**
   * Преобразует дату в UTC дату (убирает смещение часового пояса)
   */
  private toUTCDate(date: Date): Date {
    return new Date(Date.UTC(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      12, 0, 0, 0
    ));
  }

  /**
   * Форматирует дату в ISO строку (YYYY-MM-DD)
   */
  private formatToISODate(date: Date): string {
    const year = date.getUTCFullYear();
    const month = String(date.getUTCMonth() + 1).padStart(2, '0');
    const day = String(date.getUTCDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  // Метод для очистки выбранного диапазона
  clearSelection(): void {
    this.selectedPreset = null;
    this.currentDisplayValue = '';
    this.updateInputDisplay();

    this.dateRangeChange.emit({
      startDate: '',
      endDate: '',
      selectedDates: [],
      displayValue: ''
    });
  }

  private parseDate(date: string | Date): Date {
    if (typeof date === 'string') {
      if (date.match(/^\d{4}-\d{2}-\d{2}$/)) {
        const [year, month, day] = date.split('-').map(Number);
        return new Date(Date.UTC(year, month - 1, day, 12, 0, 0, 0));
      }
      return new Date(date);
    }
    return date;
  }

  private formatDateRange(startDate: Date, endDate: Date): string {
    const displayFormat = new Intl.DateTimeFormat('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
    const startStr = displayFormat.format(startDate);
    const endStr = displayFormat.format(endDate);
    return `${startStr} - ${endStr}`;
  }
}