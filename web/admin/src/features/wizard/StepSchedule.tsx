
import { Button, Input, Label } from '../../components/ui'
import type { CampaignDraft, DraftErrors } from './campaignDraft'

interface Props {
  draft: CampaignDraft
  errors: DraftErrors
  onChange: (values: Partial<CampaignDraft>) => void
  onNext: () => void
  onBack: () => void
}

export function StepSchedule({
  draft,
  errors,
  onChange,
  onNext,
  onBack,
}: Props) {
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <Label htmlFor="start">First day</Label>
          <Input
            id="start"
            type="date"
            value={draft.startDate}
            onChange={e => onChange({ startDate: e.target.value })}
            aria-invalid={!!errors.startDate}
          />
          {errors.startDate && (
            <p className="mt-1 text-sm text-red-600">{errors.startDate}</p>
          )}
        </div>

        <div>
          <Label htmlFor="end">Last day</Label>
          <Input
            id="end"
            type="date"
            value={draft.endDate}
            min={draft.startDate || undefined}
            onChange={e => onChange({ endDate: e.target.value })}
            aria-invalid={!!errors.endDate}
          />
          {errors.endDate && (
            <p className="mt-1 text-sm text-red-600">{errors.endDate}</p>
          )}
        </div>
      </div>

      <div>
        <Label htmlFor="budget">Budget (ZAR)</Label>
        <Input
          id="budget"
          inputMode="decimal"
          value={draft.budget}
          onChange={e => onChange({ budget: e.target.value })}
          placeholder="e.g. 150000"
          aria-invalid={!!errors.budget}
        />
        {errors.budget && (
          <p className="mt-1 text-sm text-red-600">{errors.budget}</p>
        )}
      </div>

      <div className="flex justify-between gap-3">
        <Button variant="secondary" onClick={onBack}>Back</Button>
        <Button onClick={onNext}>Next</Button>
      </div>
    </div>
  )
}
