import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, forwardRef, ElementRef, ViewChild, HostListener } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface Option {
  value: string;
  label: string;
}

@Component({
  selector: 'app-select',
  imports: [CommonModule],
  templateUrl: './select.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true
    }
  ]
})
export class SelectComponent implements ControlValueAccessor {
  @Input() options: Option[] = [];
  @Input() placeholder: string = 'Выберите значение';
  @Input() className: string = '';
  @Input() searchable: boolean = false;
  @Input() disabled: boolean = false;
  
  @ViewChild('dropdownContainer') dropdownContainer!: ElementRef;
  @ViewChild('searchInput') searchInput!: ElementRef;

  value: string = '';
  isOpen = false;
  searchQuery: string = '';
  filteredOptions: Option[] = [];

  // ControlValueAccessor методы
  onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  ngOnInit() {
    this.filteredOptions = [...this.options];
  }

  ngOnChanges() {
    this.filterOptions();
  }

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  // Основные методы компонента
  toggleDropdown(): void {
    if (this.disabled) return;
    
    this.isOpen = !this.isOpen;
    if (this.isOpen && this.searchable) {
      setTimeout(() => {
        this.searchInput?.nativeElement.focus();
      }, 0);
    }
  }

  selectOption(option: Option): void {
    this.value = option.value;
    this.onChange(this.value);
    this.onTouched();
    this.isOpen = false;
    this.searchQuery = '';
    this.filterOptions();
  }

  onSearchChange(event: Event): void {
    this.searchQuery = (event.target as HTMLInputElement).value;
    this.filterOptions();
  }

  filterOptions(): void {
    if (!this.searchQuery) {
      this.filteredOptions = [...this.options];
    } else {
      const query = this.searchQuery.toLowerCase();
      this.filteredOptions = this.options.filter(option =>
        option.label.toLowerCase().includes(query) ||
        option.value.toLowerCase().includes(query)
      );
    }
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.filterOptions();
    this.searchInput?.nativeElement.focus();
  }

  get selectedOption(): Option | undefined {
    return this.options.find(option => option.value === this.value);
  }

  get displayValue(): string {
    return this.selectedOption?.label || this.placeholder;
  }

  // Закрытие dropdown при клике вне компонента
  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event): void {
    if (this.isOpen && 
        this.dropdownContainer && 
        !this.dropdownContainer.nativeElement.contains(event.target)) {
      this.isOpen = false;
      this.searchQuery = '';
      this.filterOptions();
    }
  }

  @HostListener('document:keydown.escape')
  onEscapePress(): void {
    if (this.isOpen) {
      this.isOpen = false;
      this.searchQuery = '';
      this.filterOptions();
    }
  }
}
