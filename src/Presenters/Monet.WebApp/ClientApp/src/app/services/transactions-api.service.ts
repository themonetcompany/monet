import { Injectable } from '@angular/core';
import { Observable, delay, from, map } from 'rxjs';

import { expenseCategories, incomeCategories } from '../data/categories.data';

export interface ParsedOfxTransaction {
  label: string;
  amount: number;
  type: 'income' | 'expense';
  categoryId: string;
  dateLabel: string;
}

@Injectable({ providedIn: 'root' })
export class TransactionsApiService {
  importOfx(files: File[]): Observable<ParsedOfxTransaction[]> {
    return from(this.readFiles(files)).pipe(
      delay(400),
      map((contents) => this.parseContents(contents))
    );
  }

  private async readFiles(files: File[]): Promise<Array<{ name: string; content: string }>> {
    const contents = await Promise.all(files.map(async (file) => ({ name: file.name, content: await file.text() })));
    return contents;
  }

  private parseContents(files: Array<{ name: string; content: string }>): ParsedOfxTransaction[] {
    const parsed: ParsedOfxTransaction[] = [];
    files.forEach((file) => {
      const blocks = file.content.split(/<STMTTRN>/gi).slice(1);
      if (blocks.length === 0) {
        parsed.push({
          label: `Import OFX · ${file.name}`,
          amount: 0,
          type: 'expense',
          categoryId: expenseCategories[0]?.id ?? '',
          dateLabel: 'À confirmer'
        });
        return;
      }

      blocks.forEach((block) => {
        const amountRaw = this.extractTag(block, 'TRNAMT');
        const nameRaw = this.extractTag(block, 'NAME') ?? this.extractTag(block, 'MEMO') ?? file.name;
        const dateRaw = this.extractTag(block, 'DTPOSTED') ?? '';
        const amount = Number(amountRaw?.replace(',', '.') ?? '0');

        const type: 'income' | 'expense' = amount >= 0 ? 'income' : 'expense';
        const amountValue = Math.abs(amount);
        const categoryId =
          type === 'income'
            ? incomeCategories[0]?.id ?? ''
            : expenseCategories[0]?.id ?? '';

        parsed.push({
          label: nameRaw.trim(),
          amount: Number.isNaN(amountValue) ? 0 : amountValue,
          type,
          categoryId,
          dateLabel: this.formatDateLabel(dateRaw)
        });
      });
    });
    return parsed;
  }

  private extractTag(block: string, tag: string): string | null {
    const match = block.match(new RegExp(`<${tag}>([^\n\r<]+)`, 'i'));
    return match?.[1] ?? null;
  }

  private formatDateLabel(raw: string): string {
    const cleaned = raw.replace(/[^0-9]/g, '');
    if (cleaned.length < 8) {
      return 'Date inconnue';
    }
    const year = cleaned.slice(0, 4);
    const month = cleaned.slice(4, 6);
    const day = cleaned.slice(6, 8);
    const monthNames = ['janv.', 'févr.', 'mars', 'avr.', 'mai', 'juin', 'juil.', 'août', 'sept.', 'oct.', 'nov.', 'déc.'];
    const monthIndex = Number(month) - 1;
    const monthLabel = monthNames[monthIndex] ?? month;
    return `${day} ${monthLabel} ${year}`;
  }
}
