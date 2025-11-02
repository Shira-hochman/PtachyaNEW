import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HealthDeclaration } from './health-declaration';

describe('HealthDeclaration', () => {
  let component: HealthDeclaration;
  let fixture: ComponentFixture<HealthDeclaration>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthDeclaration]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HealthDeclaration);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
